using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Visa;
using Ivi.Visa;
using System.Threading;

namespace CS_visa
{
    class Session
    {
        public Session(IVisaSession session, string name)
        {
            this.session = session;
            this.sessionName = name;
        }
        public IVisaSession session;
        public string sessionName;

        //public delegate void Ses(string s);
        //public event Ses SesEv;
    }

    class MyException : Exception 
    {
        public MyException(string s) : base(s) { }
    }

    public class VISA_control
    {
        private ResourceManager rmSession;
        private IEnumerable<string> instrList;
        private List<Session> sessions = new List<Session>();
        private List<bool> connected = new List<bool>();

        public CancellationToken token;
        public char div;
        public int timeSpan;

        public IEnumerable<string> InstrList
        {
            get => instrList;
        }

        public void DisplaySessions()
        {
            foreach(Session session in this.sessions)
            {
                Console.WriteLine(session.sessionName);
            }
        }

        public IVisaSession this[string name]
        {
            get
            {
                foreach(Session s in this.sessions)
                {
                    if (name == s.sessionName) return s.session;
                }
                throw new MyException("No such instrument name!");
            }
        }

        public delegate void Notify(string s);
        public event Notify NotifyEvent;
        public event Notify DataEvent;
        public VISA_control()
        {
            this.div = '\\';
            this.timeSpan = 0;
            this.rmSession = new ResourceManager();
            FindInstr("?*");
        }
        public void FindInstr(string s)
        {
            this.instrList = this.rmSession.Find(s);
        }

        public void InstrConnect(int timeout, params string[] str)
        {
            if (str.Distinct<string>().Count<string>() != str.Length) throw new MyException("Repeating arguments!");
            if (str.Length > this.instrList.Count<string>()) throw new MyException("Too much arguments!");

            int i = 1;

            foreach(string s in str)
            {
                if (!this.instrList.Contains<string>(s)) NotifyEvent($"{s}: no such instrument!");
                else
                {
                    this.sessions.Add(new Session(this.rmSession.Open(resourceName: s,
                                                                      accessModes: AccessModes.ExclusiveLock,
                                                                      timeoutMilliseconds: timeout,
                                                                      openStatus: out ResourceOpenStatus status),
                                                  i.ToString()));
                    this.connected.Add(status == ResourceOpenStatus.Success);
                    NotifyEvent(status.ToString());
                }
                i++;
            }
        }

        public void AddSession(string sessionName, string instrName, int timeout)
        {
            if (!this.instrList.Contains<string>(instrName)) NotifyEvent($"{instrName}: no such instrument!");
            else
            {
                this.sessions.Add(new Session(this.rmSession.Open(resourceName: instrName,
                                                                  accessModes: AccessModes.ExclusiveLock,
                                                                  timeoutMilliseconds: timeout,
                                                                  openStatus: out ResourceOpenStatus status),
                                              sessionName));
                this.connected.Add(status == ResourceOpenStatus.Success);
                NotifyEvent(status.ToString());
            }
        }

        private void TalkCycle(string[] commands)
        {
            foreach (string s in commands)
            {
                if (token.IsCancellationRequested) return;
                if (s.Length == 0) throw new MyException("Empty string as a command!");
                if (s.First<char>() == this.div) continue;
                string[] temp = s.Trim().Split(this.div);
                string instr = temp[0].Trim();
                string command = temp[1].Trim();

                using (MessageBasedSession mesSession = (MessageBasedSession)this[instr])
                {
                    mesSession.RawIO.Write(command);
                    Task.Delay(this.timeSpan).Wait();
                    if (command.Last<char>() == '?')
                    {
                        string ans = mesSession.RawIO.ReadString();
                        this.DataEvent(ans);
                    }
                    mesSession.RawIO.Write("*OPC?");
                    mesSession.RawIO.ReadString();
                    Task.Delay(this.timeSpan).Wait();
                }
            }
        }

        public async void InstrTalk(string[] instrList, string[] commands)
        {
            bool connection = true;
            foreach(bool flag in this.connected)
            {
                connection &= flag;
            }
            if (!connection || this.connected.Count == 0) throw new MyException("Connection problems");
            else
            {
                Task t1 = Task.Run(() => this.TalkCycle(commands));
                await t1;
            }
        }

        public void SendCommand(string command, ref MessageBasedSession mesSession)
        {
            if (command.Length == 0) throw new MyException("Empty string as a command!");
            if (command.First<char>() == this.div) return;
            mesSession.RawIO.Write(command);
            Task.Delay(this.timeSpan).Wait();
            if (command.Last<char>() == '?')
            {
                DataEvent(mesSession.RawIO.ReadString());
            }
            mesSession.RawIO.Write("*OPC?");
            mesSession.RawIO.ReadString();
            Task.Delay(this.timeSpan).Wait();
        }
    }
}

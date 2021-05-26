using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Visa;
using Ivi.Visa;
using System.IO;
using System.Threading;

namespace CS_visa
{
    public class CalibratorProcessing
    {
        private MessageBasedSession mesSesCal;
        private MessageBasedSession mesSesMM;
        private MessageBasedSession mesSesFC;
        private VISA_control vc;
        private string[,] listMM;
        private string[,] listFC;
        private int commandNumberMM;
        private int commandNumberFC;

        public char div;
        public int timeout;
        public CalibratorProcessing(VISA_control vc, string path, string MM, string FC, char div)
        {
            this.timeout = 20000;
            this.div = div;
            this.listMM = new string[2, 200];
            this.listFC = new string[2, 200];
            this.vc = vc;
            GetCommands(path, MM, FC);
            vc.DisplaySessions();
            GetSessions();
        }

        //Получить команды для калибратора, частотомера и мультиметра из файла, который находится в указанном месте
        //Первая строка - начальная настройка (импеданс, канал и т.п.)
        //Строки, следующие далее, - библиотека команд в формате: _команда '_разделитель(div)' _тип
        //Описание типа добавить в дальнейшем
        //ВСЕ НЕ ГОДИТСЯ
        private void GetCommands(string path, string MM, string FC)
        {
            using (StreamReader sR1 = new StreamReader(path + "\\" + MM + ".txt"))
            {
                this.commandNumberMM = 0;
                while (sR1.EndOfStream == false)
                {
                    if (this.listMM.GetLength(0) < this.commandNumberMM)
                    {
                        string[,] temp = new string[2, this.listMM.GetLength(1) + 100];
                        Array.Copy(this.listMM, temp, this.listMM.Length);
                    }

                    string line = sR1.ReadLine().Trim();
                    if (line.Length != 0)
                    {
                        if (line[0] != this.div)
                        {
                            this.listMM[0, this.commandNumberMM] = "value";
                            this.listMM[1, this.commandNumberMM] = line;
                        }
                        else
                        {
                            this.listMM[0, this.commandNumberMM] = "command";
                            this.listMM[1, this.commandNumberMM] = line.Substring(1).Trim();
                        }
                        this.commandNumberMM++;
                    }
                }
            }

            using (StreamReader sR2 = new StreamReader(path + "\\" + FC + ".txt"))
            {
                this.commandNumberFC = 0;
                while (sR2.EndOfStream == false)
                {
                    if (this.listFC.GetLength(0) < this.commandNumberFC)
                    {
                        string[,] temp = new string[2, this.listFC.GetLength(1) + 100];
                        Array.Copy(this.listFC, temp, this.listFC.Length);
                    }

                    string line = sR2.ReadLine().Trim();
                    if (line.Length != 0)
                    {
                        if (line[0] != this.div)
                        {
                            this.listFC[0, this.commandNumberFC] = "value";
                            this.listFC[1, this.commandNumberFC] = line;
                        }
                        else
                        {
                            this.listFC[0, this.commandNumberFC] = "command";
                            this.listFC[1, this.commandNumberFC] = line.Substring(1).Trim();
                        }
                        this.commandNumberFC++;
                    }
                }
            }
        }

        //Необходимо обеспечить IVisaSession сессии с именами (Session.sessionName) "calibrator", "MM", "FC"
        private void GetSessions()
        {
            this.mesSesCal = (MessageBasedSession)this.vc["calibrator"];
            this.mesSesMM = (MessageBasedSession)this.vc["MM"];
            //this.mesSesFC = (MessageBasedSession)this.vc["FC"];
            this.mesSesCal.TimeoutMilliseconds = this.timeout;
            this.mesSesMM.TimeoutMilliseconds  = this.timeout;
            //this.mesSesFC.TimeoutMilliseconds  = this.timeout;
        }
        public void MM()
        {
            string calibratorComand = null;
            for (int i = 0; i < this.commandNumberMM; i++)
            {
                string[] commandString = this.listMM[1, i].Trim().Split(this.div);
                
                if (i != this.commandNumberMM - 1 && this.listMM[0, i] == "command" && this.listMM[0, i + 1] == "value")
                {
                    calibratorComand = commandString[1].Trim();
                    continue;
                }
                
                switch (this.listMM[0, i])
                {
                    case "command":
                        string instr    = commandString[0].Trim();
                        string command  = commandString[1].Trim();
                        this.vc.timeSpan = commandString.Length == 3 ? Int32.Parse(commandString[2].Trim()) : 0;
                        switch (instr)
                        {
                            case "calibrator":
                                vc.SendCommand(command, ref mesSesCal);
                                break;
                            case "MM":
                                vc.SendCommand(command, ref mesSesMM);
                                break;
                        }
                        break;
                    case "value":
                        string value = commandString[0].Trim();
                        this.vc.timeSpan = commandString.Length == 2 ? Int32.Parse(commandString[1].Trim()) : 0;
                        this.MM_measure(calibratorComand, value);
                        break;
                }
            }
        }

        private void MM_measure(string calibratorCommand, string value)
        {
            vc.SendCommand(calibratorCommand + " " + value, ref mesSesCal);
            vc.SendCommand("VOLT:DC:RANGE " + value, ref mesSesMM);
            vc.SendCommand("INIT", ref mesSesMM);
            vc.SendCommand("*TRG", ref mesSesMM);
            vc.SendCommand("FETCH?", ref mesSesMM);
        }
        public void FC()
        {
            for (int i = 0; i < this.commandNumberFC; i++)
            {

            }
        }
    }
}









 /*   static class Processing11
    {
        public static void Del(object o, VisaEventArgs a)
        {
            Console.WriteLine(a.ToString());
        }
        public static string CO(IVisaSession osc, IVisaSession cal, string model)
        {
            string buf = null;
            MessageBasedSession sessio_osc = (MessageBasedSession)osc;
            IMessageBasedFormattedIO session_osc = sessio_osc.FormattedIO;
            MessageBasedSession sessio_cal = (MessageBasedSession)cal;
            IMessageBasedFormattedIO session_cal = sessio_cal.FormattedIO;

            StreamReader sR1 = new StreamReader(@"C:\Users\Пользователь\Desktop\Михаил\" + model + ".txt");
            Dictionary<string, string> commands_osc = new Dictionary<string, string>();
            while (sR1.EndOfStream == false)
            {
                string s = sR1.ReadLine();
                commands_osc.Add(s.Split(';')[1].Trim(), s.Split(';')[0].Trim());
            }

            StreamReader sR2 = new StreamReader(@"C:\Users\Пользователь\Desktop\Михаил\9500_commands.txt");
            Dictionary<string, string> commands_9500 = new Dictionary<string, string>();
            while (sR2.EndOfStream == false)
            {
                string s = sR2.ReadLine();
                commands_9500.Add(s.Split(';')[1].Trim(), s.Split(';')[0].Trim());
            }


            StreamReader sR3 = new StreamReader(@"C:\Users\Пользователь\Desktop\Михаил\" + model + "_co.txt");
            List<string> strLs3 = new List<string>();
            while (sR3.EndOfStream == false)
            {
                strLs3.Add(sR3.ReadLine());
            }

            switch (model)
            {
                case "34401":
                    /*IVisaAsyncResult res = null;
                    res = sessio_osc.RawIO.BeginWrite(commands_9500["impedance"] + " " + "1E6");
                    sessio_osc.RawIO.EndWrite(res);
                    res = sessio_osc.RawIO.BeginWrite(commands_9500["shape"] + " " + "DC");
                    sessio_osc.RawIO.EndWrite(res);
                    res = sessio_osc.RawIO.BeginWrite(commands_9500["voltage"] + " " + "0.001");
                    sessio_osc.RawIO.EndWrite(res);
                    res = sessio_osc.RawIO.BeginWrite(commands_9500["output"] + " " + "ON");
                    sessio_osc.RawIO.EndWrite(res);

                    foreach (string s in strLs3)
                    {
                        Console.WriteLine(s);
                        res = sessio_cal.RawIO.BeginWrite(commands_9500["voltage"] + " " + s.Trim());
                        sessio_cal.RawIO.EndWrite(res);
                        Thread.Sleep(500);
                        res = sessio_osc.RawIO.BeginWrite(commands_osc["read"]);
                        sessio_cal.RawIO.EndWrite(res);
                        res = sessio_osc.RawIO.BeginRead(1024);
                        string te = sessio_osc.RawIO.EndReadString(res);
                        Console.WriteLine(te);
                    }*//*
                    cal.TimeoutMilliseconds = 5000;
                    osc.TimeoutMilliseconds = 5000;
                    cal.EnableEvent(EventType.ServiceRequest);
                    osc.EnableEvent(EventType.ServiceRequest);
                    MyFormatter myFormatter = new MyFormatter();
                    session_cal.TypeFormatter = (ITypeFormatter)myFormatter;
                    session_osc.PrintfAndFlush("%s %s", "TRIGger:SOURce", "BUS");
                    session_osc.PrintfAndFlush("%s %d", "TRIGger:DELay", 2);
                    session_osc.PrintfAndFlush("func \"volt:dc\"");
                    session_osc.PrintfAndFlush("%s %s", "volt:dc:res", "min");
                    session_osc.PrintfAndFlush("*OPC?");
                    session_osc.ReadString();
                    session_cal.PrintfAndFlush("%s %d", commands_9500["impedance"], 1E6);
                    //cal.WaitOnEvent(EventType.ServiceRequest, 2000);
                    session_cal.PrintfAndFlush("*OPC?");
                    session_cal.ReadString();
                    session_cal.PrintfAndFlush("%s %s", commands_9500["shape"], CurType.DC);
                    //cal.WaitOnEvent(EventType.ServiceRequest, 2000);
                    session_cal.PrintfAndFlush("*OPC?");
                    session_cal.ReadString();
                    session_cal.PrintfAndFlush("%s %f", commands_9500["voltage"], 0.001);
                    //cal.WaitOnEvent(EventType.ServiceRequest, 2000);
                    session_cal.PrintfAndFlush("*OPC?");
                    session_cal.ReadString();
                    session_cal.PrintfAndFlush("%s %s", commands_9500["output"], "ON");
                    //cal.WaitOnEvent(EventType.ServiceRequest, 2000);
                    session_cal.PrintfAndFlush("*OPC?");
                    session_cal.ReadString();
                    foreach (string s in strLs3)
                    {
                        session_cal.PrintfAndFlush("%s %s", commands_9500["voltage"], s.Trim());
                        session_cal.PrintfAndFlush("*OPC?");
                        session_cal.ReadString();
                        session_osc.PrintfAndFlush("%s %s", "volt:dc:range", s.Trim());
                        session_osc.PrintfAndFlush("*OPC?");
                        session_osc.ReadString();
                        //Thread.Sleep(2000);
                        //cal.WaitOnEvent(EventType.ServiceRequest, 2000);
                        session_osc.PrintfAndFlush("init");
                        //osc.WaitOnEvent(EventType.ServiceRequest, 2000
                        session_osc.PrintfAndFlush("*OPC?");
                        session_osc.ReadString();
                        session_osc.PrintfAndFlush("*TRG");
                        session_osc.PrintfAndFlush("*OPC?");
                        session_osc.ReadString();
                        session_osc.PrintfAndFlush("fetch?");
                        buf += session_osc.ReadString() + "\n";
                        session_osc.PrintfAndFlush("*OPC?");
                        session_osc.ReadString();
                    }
                    session_cal.PrintfAndFlush("%s %s",commands_9500["output"],"OFF");
                    //session_cal.ServiceRequest += Del;
                    //cal.EnableEvent(EventType.ServiceRequest);
                    //osc.EnableEvent(EventType.ServiceRequest);
                    /*session_cal.RawIO.Write(commands_9500["impedance"] + " " + "1E6");
                    Thread.Sleep(500);
                    session_cal.RawIO.Write(commands_9500["shape"] + " " + "DC");
                    Thread.Sleep(500);
                    session_cal.RawIO.Write(commands_9500["voltage"] + " " + "0.001");
                    Thread.Sleep(500);
                    session_cal.RawIO.Write(commands_9500["output"] + " " + "ON");
                    Thread.Sleep(500);
                    foreach (string s in strLs3)
                    {
                        Console.WriteLine(s);
                        session_cal.RawIO.Write(commands_9500["voltage"] + " " + s.Trim());
                        Thread.Sleep(500);
                        session_osc.RawIO.Write(commands_osc["read"]);
                        Thread.Sleep(500);
                        buf += session_osc.RawIO.ReadString();
                        Thread.Sleep(500);
                    }
                    session_cal.RawIO.Write(commands_9500["output"] + " " + "OFF");*//*
                    break;
            }
            return buf;
        }
    }
}*/

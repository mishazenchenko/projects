using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.Visa;
using System.IO;
using Word = Microsoft.Office.Interop.Word;

namespace CS_visa
{
    class Program
    {

        static void Main(string[] args)
        {
            VISA_control vc = new VISA_control();
            vc.NotifyEvent += Console.WriteLine;
            vc.DataEvent += Console.WriteLine;
            int i = 1;
            foreach(string s in vc.InstrList)
            {
                Console.WriteLine(i.ToString() + ": " + s);
                i++;
            }
            Console.WriteLine();
            Console.WriteLine("Введите номер прибора из списка выше: (сначала калибратор, затем мультиметр)");
            string input = Console.ReadLine();
            string[] splitInput = input.Split(',');
            string[] instrAr = vc.InstrList.ToArray<string>();
            vc.AddSession("calibrator", instrAr[Int32.Parse(splitInput[0]) - 1], 1000);
            vc.AddSession("MM", instrAr[Int32.Parse(splitInput[1])-1], 1000);
            vc.DisplaySessions();
            CalibratorProcessing cp = new CalibratorProcessing(vc, @"C:\Users\Пользователь\Desktop\Михаил", "34401", "34401", '\\');
            cp.MM();

            //vc.InstrConnect(Int16.Parse(input));
            //vc.InstrConnect(4,5);
            /*StreamReader sR = new StreamReader(@"C:\Users\Пользователь\Desktop\Михаил\commands.txt");
            List<string> strLs = new List<string>();
            while (sR.EndOfStream == false)
            {
                strLs.Add(sR.ReadLine());
            }
            string[] strAy = strLs.ToArray();*/
            /*string[] strAy = new string[] { 
                "1\\*IDN?",
                "2\\*IDN?",
                "1\\SYSTEM:ERROR?",
                "2\\SYSTEM:ERROR?", 
                "2\\SOURce:FUNCtion:SHAPe DC",
                "2\\ROUTe:SIGNal: IMPedance 100",
                "2\\SOURce:VOLTage:LEVel:IMMediate:AMPLitude 0.001",
                "2\\OUTPut:STATe ON",
                "1\\READ?",
                "2\\SOURce:VOLTage:LEVel:IMMediate:AMPLitude 0.005",
                "1\\READ?",
                "2\\OUTPut:STATe OFF",

            };
            Console.WriteLine("\nОтвет прибора:");
            vc.InstrTalk(strAy);*/

            //CalibratorProcessing CP = new CalibratorProcessing(vc, @"C:\Users\Пользователь\Desktop\Михаил", "34401.txt", "ddsffs", '\\');
            //CP.MM();

            //string buf = Processing.CO(vc.sessions[0], vc.sessions[1], "34401");
            //StreamWriter sW = new StreamWriter(@"C:\Users\Пользователь\Desktop\Михаил\sss.txt");
            //sW.Write(buf);// vc.Bufer);
            //sW.Close();

            //Word.Application wa = new Word.Application();
            //wa.Visible = false;
            //Microsoft.Office.Interop.Word.Document doc = wa.Documents.Add(Type.Missing, false, Word.WdNewDocumentType.wdNewBlankDocument, true);
            //wa.Selection.Text = buf;//vc.Bufer;
            //doc.SaveAs(@"C:\Users\Пользователь\Desktop\Михаил\new_doc.docx");
            //doc.Close();
            //wa.Quit();
        }
    }
}

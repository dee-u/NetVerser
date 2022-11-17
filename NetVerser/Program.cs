//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace NetVerser
//{
//    public class Program
//    {
//        /// <summary>
//        /// The main entry point for the application.
//        /// </summary>
//        /// 
//        public static bool firstInstance;
//        public static System.Threading.Mutex mutex;

//        [System.STAThreadAttribute()]
//        static void Main()
//        {
//            NetVerser.App app = new App();            

//            mutex = new System.Threading.Mutex(false, "Local\\NetVerser", out firstInstance);
//            if (!firstInstance)
//            {
//                System.Windows.MessageBox.Show("You are only allowed to run one instance of the program.   ", "Already Running");
//                app.Shutdown();
//            }
//            else
//            {
//                app.InitializeComponent();
//                app.Run();
//            }
//        }
//    }
//}

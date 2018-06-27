using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace DystopiaPostBuild
{
    public partial class MainWindow : Form
    {
        public string WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory + "\\";

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                WorkingDirectory = Environment.GetCommandLineArgs().GetValue(1).ToString();
            }
            catch (Exception e)
            {

            }

            BuildNumberResult bn = BuildNumber();
            if (bn.result)
            {
                CloseButton.Visible = true;
                StatusLabel.Text = "Incrementation finished! New build number: " + bn.NewBuildNumber;
                CloseTimer.Enabled = true;
            }
            else
            {
                CloseButton.Visible = true;
                StatusLabel.Text = "Incrementation failed!";
                AddToLog(String.Format("Build failed:" + Environment.NewLine + "GAME_VERSION_FILE path: {0}" + Environment.NewLine + "GAME_BUILD_FILE path: {1}", bn.GAME_VERSION_FILE, bn.GAME_BUILD_FILE));
            }
        }

        public void AddToLog(string logcontent)
        {
            logBox.Text += "\n" + logcontent;
        }

        public BuildNumberResult BuildNumber()
        {
            // Increment the build number.
            BuildNumberResult res = new BuildNumberResult();

            string GAME_VERSION_FILE = Path.Combine(WorkingDirectory, @"Source\DystopiaGame\Public\DystopiaBuildVersion.h");
            string GAME_BUILD_FILE = Path.Combine(WorkingDirectory, @"Source\DystopiaGame\DystopiaBuild.txt");

            res.GAME_VERSION_FILE = GAME_VERSION_FILE;
            res.GAME_BUILD_FILE = GAME_BUILD_FILE;

            string BUILD_NUMBER_TEXT = "#define GAME_BUILD_NUMBER";
            int LINE_NUMBER_TO_REPLACE = 9;

            bool bFilesExist = File.Exists(GAME_VERSION_FILE) && File.Exists(GAME_BUILD_FILE);

            if (bFilesExist)
            {
                FileInfo VFInfo = new FileInfo(GAME_VERSION_FILE);
                FileInfo BFInfo = new FileInfo(GAME_BUILD_FILE);

                VFInfo.IsReadOnly = false;
                BFInfo.IsReadOnly = false;

                string GAME_BUILD_CONTENT = File.ReadAllText(GAME_BUILD_FILE);
                int GAME_BUILD_NUMBER = int.Parse(GAME_BUILD_CONTENT);
                int INCRNUMBER = GAME_BUILD_NUMBER + 1;
                string GAME_VERSION_CONTENT = File.ReadAllText(GAME_VERSION_FILE);

                if (GAME_VERSION_CONTENT.Contains(BUILD_NUMBER_TEXT))
                {
                    int TotalLinesInGameVersionFile = File.ReadAllLines(GAME_VERSION_FILE).GetLength(0) + 1;

                    using (var sr = new StreamReader(GAME_VERSION_FILE))
                    {
                        for (int i = 1; i < TotalLinesInGameVersionFile; i++)
                        {
                            if (sr.ReadLine().Contains(BUILD_NUMBER_TEXT))
                            {
                                AddToLog("New build number generated: " + INCRNUMBER.ToString());
                                sr.Close();

                                var GameVersionFileLines = new List<string>(File.ReadAllLines(GAME_VERSION_FILE));

                                GameVersionFileLines.RemoveAt(LINE_NUMBER_TO_REPLACE);
                                GameVersionFileLines.Insert(LINE_NUMBER_TO_REPLACE, (BUILD_NUMBER_TEXT + " " + INCRNUMBER));

                                File.WriteAllLines(GAME_VERSION_FILE, GameVersionFileLines);
                                File.WriteAllText(GAME_BUILD_FILE, INCRNUMBER.ToString());

                                res.result = true;
                                res.NewBuildNumber = INCRNUMBER.ToString();
                                return res;
                            }
                        }
                    }
                }

                res.result = false;
                return res;
            }
            else
            {
                res.result = false;
                return res;
            }
        }

        public class BuildNumberResult
        {
            public bool result
            { get; set; }
            public string GAME_VERSION_FILE
            { get; set; }
            public string GAME_BUILD_FILE
            { get; set; }
            public string NewBuildNumber
            { get; set; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

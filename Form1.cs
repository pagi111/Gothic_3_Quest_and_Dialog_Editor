using Microsoft.WindowsAPICodePack.Controls.WindowsForms;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static Gothic_3_Quest_and_Dialog_Editor.Form1;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Gothic_3_Quest_and_Dialog_Editor
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Class downloaded from SO thread: https://stackoverflow.com/questions/248603/natural-sort-order-in-c-sharp
        /// It allows to sort an array in a Windows style, natural order (e.g. 1, 2, 3, 11, 110, 210, 310) 
        /// instead of original C# way (e.g. 1, 11, 110, 2, 210, 3, 310)
        /// <para></para>
        /// Use like this: var sortedArray = originalArray.OrderBy(x => x, new NaturalStringComparer());
        /// </summary>
        public class NaturalStringComparer : IComparer<string>
        {
            private static readonly Regex _re = new Regex(@"(?<=\D)(?=\d)|(?<=\d)(?=\D)", RegexOptions.Compiled);

            public int Compare(string x, string y)
            {
                x = x.ToLower();
                y = y.ToLower();
                if (string.Compare(x, 0, y, 0, Math.Min(x.Length, y.Length)) == 0)
                {
                    if (x.Length == y.Length) return 0;
                    return x.Length < y.Length ? -1 : 1;
                }
                var a = _re.Split(x);
                var b = _re.Split(y);
                int i = 0;
                while (true)
                {
                    int r = PartCompare(a[i], b[i]);
                    if (r != 0) return r;
                    ++i;
                }
            }

            private static int PartCompare(string x, string y)
            {
                int a, b;
                if (int.TryParse(x, out a) && int.TryParse(y, out b))
                    return a.CompareTo(b);
                return x.CompareTo(y);
            }
        }

        public Form1()
        {
            InitializeComponent();
            textLanguageList.Items.AddRange(languages);
            textLanguageList.SelectedItem = "English";
            ReadHistoryFile();
            DataPanels_Controls_AddEditMethods();
            resizeToInfosPictureBox.Location = new System.Drawing.Point(resizeToInfosPictureBox.Location.X, 300);
            resizeToQuestsPictureBox.Location = new System.Drawing.Point(resizeToQuestsPictureBox.Location.X, 300);

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CreateHistoryFile();
        }

        #region Cross-thread calls
        //private delegate void SafeListCallDelegate(string text);
        //private Thread thread2 = null;
        ////button1.Click += new EventHandler(Button1_Click);

        //private async void button1_Click(object sender, EventArgs e)
        //{
        //    thread2 = new Thread(new ThreadStart(SetList));
        //    await System.Threading.Tasks.Task.Run(() => thread2.Start());
        //    Thread.Sleep(1000);
        //}

        //private void SetList()
        //{
        //    string[] files = Directory.GetFiles(infoFilesFolder);
        //    infoFilesList.Items.Clear();
        //    foreach (string file in files)
        //    {
        //        string textToAdd = Path.GetFileName(file).Replace(infoFileExtension, "");
        //        WriteListSafe(textToAdd);
        //    }

        //}

        //private void WriteListSafe(string text)
        //{
        //    if (infoFilesList.InvokeRequired)
        //    {
        //        var d = new SafeListCallDelegate(WriteListSafe);
        //        infoFilesList.Invoke(d, new object[] { text });

        //    }
        //    else
        //    {
        //        infoFilesList.Items.Add(text);

        //    }
        //}
        #endregion

        #region Classes
        public class QuestOrInfoFile
        {
            public bool wasModified = false;
            public string FileName;
            public string FilePath;

            public string Name;

            public QuestOrInfoFile() { }
            
            /// <summary>
            /// This constructor allows to create a new file as a copy of another file (meaning all fields' values are copied over to the new file)
            /// <para>Still, the problem is that some fields that require the new keyword are just copies of the copied object's fields
            /// (so they are not separate entities; if they are modiefied, the same fields in the old object are also modified)</para>
            /// </summary>
            public QuestOrInfoFile(QuestOrInfoFile fileToCopyFrom)
            {
                foreach (FieldInfo copyField in fileToCopyFrom.GetType().GetFields())
                {
                    FieldInfo thisField = this.GetType().GetField(copyField.Name);
                    thisField.SetValue(this, copyField.GetValue(fileToCopyFrom));
                }
            }
        }

        public class QuestFile : QuestOrInfoFile
        {
            //public bool wasModified = false;
            //public string FileName;
            //public string FilePath;

            //public string Name;
            public string LogTopic;
            public string FinishedQuests;
            public string Folder;
            public string DeliveryEntities;
            public string DeliveryAmounts;
            public string DeliveryCounter;
            public string DestinationEntity;
            public string Type;
            public string ExperiencePoints;
            public string PoliticalSuccess;
            public string PoliticalSuccessAmount;
            public string PoliticalFailure;
            public string PoliticalFailureAmount;
            public string EnclaveSuccess;
            public string EnclaveSuccessAmount;
            public string EnclaveFailure;
            public string EnclaveFailureAmount;
            public string AttribSuccess;
            public string AttribSuccessAmount;
            public string RunningTimeYears;
            public string RunningTimeDays;
            public string RunningTimeHours;

            public string[] TextsByLanguage;

            public QuestFile() { }

            public QuestFile(QuestOrInfoFile fileToCopyFrom) : base(fileToCopyFrom) { }
        }

        public class InfoFile : QuestOrInfoFile
        {
            //public bool wasModified = false;
            public bool dialogsLoaded = false;
            //public string FileName;
            //public string FilePath;

            //public string Name;
            public string SortID;
            public string Owner;
            public string Parent;
            public string Quest;
            public string Folder;
            public string InfoGiven;
            public string ClearChildren;
            public string Permanent;
            public string GoldCost;                                                        //Held
            public string ConditionType;
            public string InfoType;

            public string CondOwnerNearEntity;                                              //other
            public string CondPlayerKnows;          //Can have multiple entries             Held
            public string CondPlayerKnowsNot;       //Can have multiple entries
            public string CondItemContainer;        //Can have multiple entries             //other
            public string CondItems;                //Can have multiple entries
            public string CondItemAmounts;          //Can have multiple entries
            public string CondWearsItem;            //Can have multiple entries            
            public string CondSecondaryNPC;         //Can have multiple entries             //other
            public string CondSecondaryNPCstates;   //0 – lebendig
                                                    //1 – wurde nicht vom Helden besiegt
                                                    //2 – wurde vom Helden besiegt
                                                    //3 – tot
                                                    //4 – ist dem Helden schon bekannt(der Held hat bereits mit dem NPC gesprochen oder es versucht)
                                                    //5 - ist dem Helden unbekannt
            public string CondHasSkill;             //Can have multiple entries             Held
            public string CondPAL;                  //Can have multiple entries             NPC
            public string CondReputGroup;           //z.B. CondReputGroup=MoraSul;Reb       Held
            public string CondReputAmount;          //z.B. CondReputAmount=70;12
            public string CondReputRelation;        //z.B. CondReputRelation=MAX;MIN
            public string TeachSkill;               //z.B. TeachSkill=Perk_Druid
            public string TeachAttrib;              //z.B. TeachAttrib=ALC
            public string TeachAttribValue;         //z.B. TeachAttribValue=1

            public Dictionary<string, string[]> Dialogs = new Dictionary<string, string[]>(); //<string> = the dialog code (InfoScript_Texts), <string[]> = array with dialog translations by language from stringtable.ini
            public string[] InfoScript_Commands;
            public string[] InfoScript_Entities1;
            public string[] InfoScript_Entities2;
            public string[] InfoScript_IDs1;
            public string[] InfoScript_IDs2;
            public string[] InfoScript_Texts;

            public InfoFile() { }

            public InfoFile(QuestOrInfoFile fileToCopyFrom) : base(fileToCopyFrom) { }

        }

        public static class HistoryFile //Just to see if it works better
        {
            public static string FilePath = Path.Combine(baseDir, "History.txt");
            public static List<string> questFoldersHistoryList = new List<string>();
            public static List<string> infoFoldersHistoryList = new List<string>();
            public static void CreateHistoryFile()
            {
                using (StreamWriter writer = new StreamWriter(historyFile))
                {
                    writer.WriteLine("Quest folders:");
                    foreach (string folder in questFoldersHistoryList)
                    {
                        writer.WriteLine(folder);
                    }
                    writer.WriteLine("Info folders:");
                    foreach (string folder in infoFoldersHistoryList)
                    {
                        writer.WriteLine(folder);
                    }
                }
            }

            public static void ReadHistoryFile(ComboBox questFolderCbx, ComboBox infosFolderCbx)
            {
                if (File.Exists(historyFile))
                {
                    using (StreamReader reader = new StreamReader(historyFile))
                    {
                        string line;
                        int part = 0;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Equals("Quest folders:"))
                            {
                                part = 1;
                                continue;
                            }
                            if (line.Equals("Info folders:"))
                            {
                                part = 2;
                                continue;
                            }
                            if (part == 1) { questFoldersHistoryList.Add(line); }
                            if (part == 2) { infoFoldersHistoryList.Add(line); }
                        }
                    }
                }
                questFolderCbx.Items.AddRange(questFoldersHistoryList.ToArray());
                infosFolderCbx.Items.AddRange(infoFoldersHistoryList.ToArray());
            }
        }
        #endregion


        #region General Variables
        string[] languages = { "English", "Italian", "French", "German", "Spanish",
                                "Czech", "Hungarian", "Polish", "Russian", "TRC" };

        const string questFileExtension = "_quest_G3_World_01.quest";
        const string infoFileExtension = "_info_G3_World_01.info";

        static string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        static string historyFile = Path.Combine(baseDir, "History.txt");

        
        List<string> questFoldersHistoryList = new List<string>();
        List<string> infoFoldersHistoryList = new List<string>();

        string questFilesFolder;
        string infoFilesFolder;
        string stringtableIniPath;
        string currentInfoFileOwner;
        InfoFile currentInfoFile;
        QuestFile currentQuestFile;

        Dictionary<string, QuestFile> dictQuestFiles = new Dictionary<string, QuestFile>();
        Dictionary<string, InfoFile> dictInfoFiles = new Dictionary<string, InfoFile>();
        #endregion


        Dictionary<string, string[]> dictStringtable = new Dictionary<string, string[]>();
        string[] first5linesFromStringtable = new string[5];

        List<string> locAdmin_Languages = new List<string>();
        List<string> locAdmin_Revisions = new List<string>();
        
        
        
        #region Stringtable Methods
        private void LoadStringtableToDict()
        {
            using (StreamReader reader = new StreamReader(stringtableIniPath))
            {
                string line; int part = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("[LocAdmin_Languages]")) { part = 1; }
                    if (line.Contains("[LocAdmin_Strings]")) 
                    {
                        locAdmin_Languages.Add(line);
                        part = 2;
                        continue;
                    }
                    if (line.Contains("[LocAdmin_Revisions]")) { part = 3; }

                    switch (part) {
                        case 1:
                            locAdmin_Languages.Add(line);
                            break;
                        case 2:
                            string[] lineSplit = line.Split('=');
                            if (lineSplit.Length != 2) { continue; }
                            string[] lineTexts = lineSplit[1].Split(new[] { ";;" }, StringSplitOptions.None);
                            lineTexts[lineTexts.Length - 1] = lineTexts.Last().TrimEnd(new char[] { ';' });
                            dictStringtable[lineSplit[0]] = lineTexts;
                            break;
                        case 3:
                            locAdmin_Revisions.Add(line);
                            break;
                    }
                }
            }
        }

        private void PrintStringtableFromDict()
        {
            if (stringtableIniPath == null) { return; }
            using (StreamWriter writer = new StreamWriter(stringtableIniPath + "new"))
            {
                foreach (string line in locAdmin_Languages)
                {
                    writer.WriteLine(line);
                }
                foreach (var entry in dictStringtable)
                {
                    writer.WriteLine(entry.Key + "=" + string.Join(";;", entry.Value) + ";");
                }
                foreach (string line in locAdmin_Revisions)
                {
                    writer.WriteLine(line);
                }
            }
            if (File.Exists(stringtableIniPath + "old")) { File.Delete(stringtableIniPath + "old"); }
            File.Move(stringtableIniPath, stringtableIniPath + "old");
            File.Move(stringtableIniPath + "new", stringtableIniPath);
        }

        private void textLanguageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(stringtableIniPath))
            {
                AddDialogsWithSameOwnerToGridView(ownerTextbox.Text, true);
                
                if (currentQuestFile != null)
                {
                    if (currentQuestFile.TextsByLanguage == null)
                    {
                        ActivateQuestFile(currentQuestFile.Name);
                    }
                    questTextBox.Text = currentQuestFile.TextsByLanguage[textLanguageList.SelectedIndex];
                }
                
                if (currentInfoFile != null)
                {
                    int i = 0;
                    foreach (var dialogCode in currentInfoFile.InfoScript_Texts)
                    {
                        if (!string.IsNullOrEmpty(dialogCode))
                        {
                            if (currentInfoFile.Dialogs[dialogCode].Length == 0) //<- This should only happen if info file was activated 
                                                                                 //    Before stringtable.ini path was entered
                            {
                                ActivateInfoFile(currentInfoFile.Name);       //<- Then simply activate the current info file again
                            }
                            infoTextsGrid[6, i].Value = currentInfoFile.Dialogs[dialogCode][textLanguageList.SelectedIndex];
                            //infoTextsGrid[6, i].Value = dictStringtable[dialogCode][textLanguageList.SelectedIndex];
                        }
                        i++;
                    }
                }
            }
        }

        private void UpdateTextsInStringtable(QuestOrInfoFile file)
        {
            if (string.IsNullOrEmpty(stringtableIniPath))
            {
                return;
            }
            if (file is QuestFile)
            {
                dictStringtable[((QuestFile)file).LogTopic] = ((QuestFile)file).TextsByLanguage;
            }
            else if (file is InfoFile)
            {
                if (((InfoFile)file).Dialogs.Count == 0)
                {
                    return;
                }

                foreach (var key in ((InfoFile)file).Dialogs.Keys)
                {
                    dictStringtable[key] = ((InfoFile)file).Dialogs[key];
                }
            }
        }
        #endregion


        #region History Methods
        private void CreateHistoryFile()
        {
            using (StreamWriter writer = new StreamWriter(historyFile))
            {
                writer.WriteLine("Quest folders:");
                foreach (string folder in questFoldersHistoryList)
                {
                    writer.WriteLine(folder);
                }
                writer.WriteLine("Info folders:");
                foreach (string folder in infoFoldersHistoryList)
                {
                    writer.WriteLine(folder);
                }
            }
        }

        private void ReadHistoryFile()
        {
            if (File.Exists(historyFile))
            {
                using (StreamReader reader = new StreamReader(historyFile))
                {
                    string line;
                    int part = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Equals("Quest folders:"))
                        {
                            part = 1;
                            continue;
                        }
                        if (line.Equals("Info folders:"))
                        {
                            part = 2;
                            continue;
                        }
                        if (part == 1) { questFoldersHistoryList.Add(line); }
                        if (part == 2) { infoFoldersHistoryList.Add(line); }
                    }
                }
            }

            infosFolderMenuItem.DropDownItems.Insert(0, new ToolStripSeparator());
            for (int i = 0; i < infoFoldersHistoryList.Count; i++)
            {
                infosFolderMenuItem.DropDownItems.Insert(i, new ToolStripMenuItem(infoFoldersHistoryList[i]));
                infosFolderMenuItem.DropDownItems[i].Click += infosFolderMenuItem_Click;
            }
            
            questsFolderMenuItem.DropDownItems.Insert(0, new ToolStripSeparator());
            for (int i = 0; i < questFoldersHistoryList.Count; i++)
            {
                questsFolderMenuItem.DropDownItems.Insert(i, new ToolStripMenuItem(questFoldersHistoryList[i]));
                questsFolderMenuItem.DropDownItems[i].Click += questsFolderMenuItem_Click;
            }
            
            //stringtableFileMenuItem.DropDownItems.Insert(0, new ToolStripSeparator());
            //for (int i = 0; i < infoFoldersHistoryList.Count; i++)
            //{
            //    stringtableFileMenuItem.DropDownItems.Insert(i, new ToolStripMenuItem(infoFoldersHistoryList[i]));
            //    stringtableFileMenuItem.DropDownItems[i].Click += stringtableFileMenuItem_Click;
            //}
        }


        private void infosFolderMenuItem_Click(object sender, EventArgs e)
        {
            var snd = sender as ToolStripMenuItem;
            if (snd.Name == "infosFolderMenuItem_ChooseNew")
            {
                var chooseInfosFolder = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog { IsFolderPicker = true };
                if (chooseInfosFolder.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
                {
                    UpdateQuestOrInfoFilesList(chooseInfosFolder.FileName, infoFilesList);
                }
            }
            else
            {
                UpdateQuestOrInfoFilesList(snd.Text, infoFilesList);
            }
            
        }
        
        private void questsFolderMenuItem_Click(object sender, EventArgs e)
        {
            var snd = sender as ToolStripMenuItem;
            if (snd.Name == "questsFolderMenuItem_ChooseNew")
            {
                var chooseQuestsFolder = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog { IsFolderPicker = true };
                if (chooseQuestsFolder.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
                {
                    UpdateQuestOrInfoFilesList(chooseQuestsFolder.FileName, questFilesList);
                }
            }
            else
            {
                UpdateQuestOrInfoFilesList(snd.Text, questFilesList);
            }
            
        }
        
        private void stringtableFileMenuItem_Click(object sender, EventArgs e)
        {
            var snd = sender as ToolStripMenuItem;
            if (snd.Name == "stringtableFileMenuItem_ChooseNew")
            {
                var chooseStringtableIniFile = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog { IsFolderPicker = false };
                if (chooseStringtableIniFile.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
                {
                    if (Path.GetFileName(chooseStringtableIniFile.FileName).ToLower() != "stringtable.ini")
                    {
                        string messageTitle = "Is the stringtable.ini filepath correct?";
                        string messageText = "The name of the file you have chosen is NOT stringtable.ini \n" +
                            "You should look for the stringtable.ini file. Continue only if you really know what you are doing. \n" +
                            "Are you sure you have chosen the correct stringtable.ini filepath and want to continue?";

                        if (System.Windows.Forms.MessageBox.Show(messageText, messageTitle, MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            return;
                        }
                    }
                    stringtableIniPath = chooseStringtableIniFile.FileName;
                    stringtableFileMenuItem.Image = imageList1.Images[1];

                    LoadStringtableToDict();
                }
            }
            else
            {
                throw new NotImplementedException();
                //TO DO
            }
        }

        /// <summary> <para>-----------------------</para> <para>folder = folder to add</para> 
        /// <para>list - is it questFoldersHistoryList or infoFoldersHistoryList</para> </summary>
        private void AddFolderToFoldersHistoryList(string folder, List<string> list) //folder to add; list - is it quest folders list or info folders list 
        {
            int maxHistoryItems = 5;
            if (list.Contains(folder)) { list.Remove(folder); }
            list.Insert(0, folder);
            if (list.Count > maxHistoryItems) { list.RemoveRange(maxHistoryItems, list.Count - maxHistoryItems); }

            ToolStripMenuItem menu;
            if (list == questFoldersHistoryList) 
                { menu = questsFolderMenuItem; }
            else if (list == infoFoldersHistoryList) 
                { menu = infosFolderMenuItem; }
            else 
                { return; }    //ADD MESSAGE

            menu.DropDownItems.Insert(0, new ToolStripMenuItem(folder));
            menu.Image = imageList1.Images[1];
        }

        #endregion

        private void saveFilesButton_Click(object sender, EventArgs e)
        {
            if (saveInfoFilesCheckBox.Checked) { SaveInfoFiles(); }
            if (saveQuestFilesCheckBox.Checked) { SaveQuestFiles(); }
            if (saveStringtableCheckBox.Checked) { PrintStringtableFromDict(); }
        }



        #region DataPanels
        private void DataPanels_Controls_AddEditMethods()
        {
            Panel[] dataPanels = new Panel[2] { questDataPanel, infoDataPanel };

            foreach (var panel in dataPanels)
            {
                foreach (var control in panel.Controls)
                {
                    if (control is RichTextBox)
                    {
                        ((RichTextBox)control).Leave += DataPanels_TextBox_Leave;
                        ((RichTextBox)control).KeyDown += DataPanels_TextBox_KeyDown;
                    }
                    else if (control is ComboBox)
                    {
                        ((ComboBox)control).SelectionChangeCommitted += DataPanels_ComboBox_SelectionChangeCommitted;
                    }
                }
            }

        }

        private void DataPanels_Controls_UpdateFields(Control control)
        {
            QuestOrInfoFile file; ListView listview; string commonPrefix = null; string folder; string fileExtension;
            if (control.Parent == questDataPanel)
            {
                file = currentQuestFile; listview = questFilesList; folder = questFilesFolder; fileExtension = questFileExtension;
                if (control is RichTextBox) { commonPrefix = "tbx_quest"; }
                else if (control is ComboBox) { commonPrefix = "cbx_quest"; }
            }
            else if (control.Parent == infoDataPanel)
            {
                file = currentInfoFile; listview = infoFilesList; folder = infoFilesFolder; fileExtension = infoFileExtension;
                if (control is RichTextBox) { commonPrefix = "tbx_info"; }
                else if (control is ComboBox) { commonPrefix = "cbx_info"; }
            }
            else
                { return; }

            FieldInfo field = file?.GetType().GetField(control.Name.Replace(commonPrefix, ""));
            if (field == null) { return; }

            if (field.Name == "Name" && control.Text != field.GetValue(file).ToString())
            {
                string title = "Create a new file or change the name of the file?";
                string text = "The Name parameter should be the same as the filename (except the standard ending)." + "\n" +
                              "It is not possible to change the Name parameter without changing the file name." + "\n" +
                              "Would you like to create a copy of this file with a new name?" + "\n" +
                              "Choosing 'Yes' will create a copy of the current file with a new name (the old file will not be changed)." + "\n" +
                              "Choosing 'No' will change the name of the existing file (the file with the old name will cease to exist)." + "\n" +
                              "Choosing 'Cancel' will revert any changes made to the file name.";
                var dialog = System.Windows.Forms.MessageBox.Show(text, title, MessageBoxButtons.YesNoCancel);
                
                if (dialog == DialogResult.Cancel)
                {
                    control.Text = field.GetValue(file).ToString();
                    return;
                }
                else
                {
                    string oldFileName = field.GetValue(file).ToString();
                    string oldFilePath = Path.Combine(folder, oldFileName + fileExtension);
                    string newFileName = control.Text;
                    string newFilePath = Path.Combine(folder, control.Text + fileExtension);

                    //OPTION 1
                    //QuestOrInfoFile newFile;

                    //if (fileExtension == questFileExtension)
                    //{
                    //    dictQuestFiles[newFileName] = new QuestFile(file as QuestFile);
                    //    newFile = dictQuestFiles[newFileName];
                    //}
                    //else
                    //{
                    //    dictInfoFiles[newFileName] = new InfoFile(file as InfoFile);
                    //    newFile = dictInfoFiles[newFileName];
                    //}
                    //if (dialog == DialogResult.No)
                    //{
                    //    File.Delete(oldFilePath);
                    //    listview.Items.Remove(listview.FindItemWithText(oldFileName));
                    //    if (fileExtension == questFileExtension) { dictQuestFiles.Remove(oldFileName); }
                    //    else { dictInfoFiles.Remove(oldFileName); }
                    //}

                    //newFile.Name = newFileName;
                    //newFile.FileName = newFileName + fileExtension;
                    //newFile.FilePath = newFilePath;
                    //PrintValueFromFields(newFile);
                    //LoadQuestOrInfoToDict(newFilePath); //It's necessary to load the file again - this way a new QuestOrInfo object is created
                    //                                   //in the dictionary and all its fields are created and assigned to anew
                    //                                  //Otherwise the problem was that some fields that required the new keyword were 
                    //                                 //created as copies of the old object's fields (the object that was copied)


                    //listview.Items.Add(control.Text);
                    //if (fileExtension == questFileExtension) { ActivateQuestFile(control.Text); }
                    //else { ActivateInfoFile(control.Text); }

                    //OPTION 2
                    if (File.Exists(newFilePath))
                    {
                        string title2 = "File with this name already exists?";
                        string text2 = "File with this name already exists." + "\n" +
                                      "Would you like to replace it with the current file?.";
                        var dialog2 = System.Windows.Forms.MessageBox.Show(text2, title2, MessageBoxButtons.OKCancel);

                        if (dialog2 == DialogResult.Cancel)
                        {
                            control.Text = field.GetValue(file).ToString();
                            return;
                        }
                        else
                        {
                            File.Delete(newFilePath);
                            listview.Items.Remove(listview.FindItemWithText(newFileName));
                        }
                    }
                    File.Copy(oldFilePath, newFilePath);

                    if (dialog == DialogResult.No)
                    {
                        File.Delete(oldFilePath);
                        listview.Items.Remove(listview.FindItemWithText(oldFileName));
                        if (fileExtension == questFileExtension) { dictQuestFiles.Remove(oldFileName); }
                        else { dictInfoFiles.Remove(oldFileName); }
                    }

                    LoadQuestOrInfoToDict(newFilePath);
                    listview.Items.Add(newFileName);

                    if (fileExtension == questFileExtension)
                    {
                        dictQuestFiles[newFileName].Name = newFileName;

                        PrintValueFromFields(dictQuestFiles[newFileName]);
                        ActivateQuestFile(newFileName);
                    }
                    else
                    {
                        dictInfoFiles[newFileName].Name = newFileName;

                        PrintValueFromFields(dictInfoFiles[newFileName]);
                        ActivateInfoFile(newFileName);
                    }

                }
                return;
            }
            field.SetValue(file, control.Text);
        }

        private void DataPanels_ComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DataPanels_Controls_UpdateFields(sender as Control);
        }

        private void DataPanels_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DataPanels_Controls_UpdateFields(sender as Control);
                e.SuppressKeyPress = true;
            }
        }

        private void DataPanels_TextBox_Leave(object sender, EventArgs e)
        {
            DataPanels_Controls_UpdateFields(sender as Control);
        }
        #endregion

        
        #region Update Quest or Info Files List
        
        private void AddOrUpdateFilesDict(string key, QuestOrInfoFile value)
        {
            if      (value is QuestFile)    { dictQuestFiles[key] = value as QuestFile; }
            else if (value is InfoFile)     { dictInfoFiles[key] = value as InfoFile; }
        }

        private void LoadInfoToInfosDict(string filePath)
        {
            InfoFile file = new InfoFile();
            file.FileName = Path.GetFileName(filePath);
            file.FilePath = filePath;

            if (!File.Exists(file.FilePath))
            {
                ShowWarning_FileNotFound(file.FilePath);
                return;
            }
            SetValueToFields(file);
            FindDialogsForInfoFile(file);
            dictInfoFiles[file.Name] = file;
        }

        private void LoadQuestToQuestDict(string filePath)
        {
            QuestFile file = new QuestFile();
            file.FileName = Path.GetFileName(filePath);
            file.FilePath = filePath;

            if (!File.Exists(file.FilePath))
            {
                ShowWarning_FileNotFound(file.FilePath);
                return;
            }

            SetValueToFields(file);
            dictQuestFiles[file.Name] = file;
        }

        private void LoadQuestOrInfoToDict(string filePath)
        {
            if (!File.Exists(filePath))
            {
                ShowWarning_FileNotFound(filePath);
                return;
            }

            QuestOrInfoFile file; string fileExtension;
            if      (filePath.Contains(questFileExtension))     { file = new QuestFile(); fileExtension = questFileExtension; }
            else if (filePath.Contains(infoFileExtension))      { file = new InfoFile(); fileExtension = infoFileExtension; }
            else                                                { return; }                 //ADD MESSAGE

            file.FileName = Path.GetFileName(filePath);
            file.FilePath = filePath;
            SetValueToFields(file);
            
            file.Name = Path.GetFileName(filePath).Replace(fileExtension, "");  //To REMOVE | Now it is necessary when creating new files
                                                                               //by renaming them in the app

            if (file is InfoFile) { FindDialogsForInfoFile(file as InfoFile); }
            AddOrUpdateFilesDict(file.Name, file);
        }
        
        private void SetValueToFields(QuestOrInfoFile file)
        {
            using (StreamReader reader = new StreamReader(file.FilePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    foreach (var field in file.GetType().GetFields())
                    {
                        if (line.Split('=')[0] == field.Name)
                        {
                            if (field.Name.StartsWith("InfoScript_"))
                            {
                                string[] array = line.Replace(field.Name, "").Replace("=", "").Split(new[] { ";" }, StringSplitOptions.None);
                                field.SetValue(file, array);    //Assigns array to the specific field of the infofile
                            }
                            else //if (!field.Name.StartsWith("InfoScript_"))
                            {
                                field.SetValue(file, line.Replace(field.Name, "").Replace("=", ""));
                            }
                            break;
                        }
                    }
                }
            }
            if (String.IsNullOrEmpty(file.Name))    //This needs to be changed as it doesn't do its job
            {
                ShowWarning_FileIsProbablyEmpty(file.FilePath);
                return;
            }
        }

        List<string> questFileFieldNamesToPrint = new List<string> {
            "Name",
            "LogTopic",
            "LogText",
            "Type",
            "ExperiencePoints",
            "FinishedQuests",
            "Folder",
            "DeliveryEntities",
            "DeliveryAmounts",
            "DeliveryCounter",
            "DestinationEntity",
            "PoliticalSuccess",
            "PoliticalSuccessAmount",
            "PoliticalFailure",
            "PoliticalFailureAmount",
            "EnclaveSuccess",
            "EnclaveSuccessAmount",
            "EnclaveFailure",
            "EnclaveFailureAmount",
            "AttribSuccess",
            "AttribSuccessAmount",
            "RunningTimeYears",
            "RunningTimeDays",
            "RunningTimeHours",
        };
        
        List<string> infoFileFieldNamesToPrint = new List<string> {
            "Name",
            "SortID",
            "ConditionType",
            "InfoType",
            "Owner",
            "Parent",
            "Quest",
            "Folder",
            "InfoGiven",
            "ClearChildren",
            "Permanent",
            "GoldCost",

            "CondOwnerNearEntity",
            "CondPlayerKnows",
            "CondPlayerKnowsNot",
            "CondItemContainer",
            "CondItems",
            "CondItemAmounts",
            "CondWearsItem",
            "CondSecondaryNPC",
            "CondSecondaryNPCstates",

            "CondHasSkill",
            "CondPAL",
            "CondReputGroup",
            "CondReputAmount",
            "CondReputRelation",
            "TeachSkill",
            "TeachAttrib",
            "TeachAttribValue",

            "InfoScript_Commands",
            "InfoScript_Entities1",
            "InfoScript_Entities2",
            "InfoScript_IDs1",
            "InfoScript_IDs2",
            "InfoScript_Texts",
        };

        List<string> infoFileFieldNamesToGrid = new List<string> {
            "Owner",
            "Parent",
            "Quest",
            "Folder",

            "CondOwnerNearEntity",
            "CondPlayerKnows",
            "CondPlayerKnowsNot",
            "CondItemContainer",
            "CondItems",
            "CondItemAmounts",
            "CondWearsItem",
            "CondSecondaryNPC",
            "CondSecondaryNPCstates",

            "CondHasSkill",
            "CondPAL",
            "CondReputGroup",
            "CondReputAmount",
            "CondReputRelation",
            "TeachSkill",
            "TeachAttrib",
            "TeachAttribValue",
        };

        private void PrintValueFromFields(QuestOrInfoFile file)
        {
            if ( !(file is QuestFile || file is InfoFile)) {
                return;
            }
            List<string> list = file is QuestFile ? questFileFieldNamesToPrint : infoFileFieldNamesToPrint; 
            string text = file is QuestFile ? "[Quest]" : "[Info]";
            
            using (StreamWriter writer = new StreamWriter(file.FilePath + "new"))
            {
                writer.WriteLine(text);
                foreach (string row in list)
                {
                    FieldInfo currentField = file.GetType().GetField(row);
                    if (currentField == null) 
                        { continue; }

                    if (currentField.FieldType == typeof(string[]))
                    {
                        var arrayFromField = currentField.GetValue(file) as string[];    //Gets the actual value of the field = string[]
                        string line = arrayFromField != null ? currentField.Name + "=" + string.Join(";", arrayFromField) : currentField.Name;
                        writer.WriteLine(line);
                    }
                    else if (currentField.FieldType == typeof(string))
                    {
                        string fieldValue = currentField.GetValue(file) as string;
                        if (!string.IsNullOrEmpty(fieldValue))  { writer.WriteLine(currentField.Name + "=" + fieldValue); }
                        else                                    { writer.WriteLine(currentField.Name); }
                    }
                }
            }
            if (File.Exists(file.FilePath)) { File.Move(file.FilePath, file.FilePath + "old"); }
            File.Move(file.FilePath + "new", file.FilePath);
            if (File.Exists(file.FilePath + "old")) { File.Delete(file.FilePath + "old"); }

        }

        private void SaveQuestFiles()       //This does not read the actual file, but just prints everything from dictInfoFiles.Values 
        {
            foreach (QuestFile value in dictQuestFiles.Values)
            {
                if (value.wasModified)
                {
                    PrintValueFromFields(value);
                    UpdateTextsInStringtable(value);
                }
            }
        }

        private void SaveInfoFiles()    //This does not read the actual file, but just prints everything from dictInfoFiles.Values
        {
            foreach (InfoFile value in dictInfoFiles.Values)
            {
                if (value.wasModified)
                {
                    PrintValueFromFields(value);
                    UpdateTextsInStringtable(value);
                }
            }
        }


        private void UpdateQuestOrInfoFilesList(string folderWithFiles, ListView listview)
        {
            if (!Directory.Exists(folderWithFiles))
            {
                System.Windows.Forms.MessageBox.Show("Folder doesn't exist: " + folderWithFiles);
                return;
            }
            
            string fileExtension;
            if (listview == questFilesList)
            {
                questFilesFolder = folderWithFiles;
                fileExtension = questFileExtension;
                AddFolderToFoldersHistoryList(folderWithFiles, questFoldersHistoryList);
            }
            else if (listview == infoFilesList)
            {
                infoFilesFolder = folderWithFiles;
                fileExtension = infoFileExtension;
                AddFolderToFoldersHistoryList(folderWithFiles, infoFoldersHistoryList);
            }
            else { return; }

            listview.Items.Clear();
            string[] files = Directory.GetFiles(folderWithFiles);
            var sortedFiles = files.OrderBy(x => x, new NaturalStringComparer());   //This sorts the array; needed to download
                                                                                    //NaturalStringComparer class from SO
            foreach (string file in sortedFiles)
            {
                //string textToAdd = file;                                              //This add to the list the whole file path
                //string textToAdd = Path.GetFileName(file);                           //This adds only the file name
                string textToAdd = Path.GetFileName(file).Replace(fileExtension, ""); //This adds only the base file name without: _info_G3_World_01.info
                LoadQuestOrInfoToDict(file);
                listview.Items.Add(textToAdd);
            }

        }

        
        private void ActivateQuestFile(string questName, bool skipQuestFolderGridReload = false)
        {
            if (questName.EndsWith("old")) { questName = questName.Remove(questName.Length - 3); }

            if (!dictQuestFiles.ContainsKey(questName))
            {
                ShowWarning_KeyInDictionaryNotFound(questName, "dictQuestFiles");
                return;
            }
            QuestFile questfile = currentQuestFile = dictQuestFiles[questName];
            questfile.wasModified = true;
            questGridView.Rows.Clear();

            //System.Drawing.Size size = new System.Drawing.Size(dataGridView1.Size.Width,
            //                            //dataGridView1.Columns[0].HeaderCell.Size.Height +
            //                            6 * dataGridView1.RowTemplate.Height);

            //dataGridView1.Size = size;

            void PrintFieldsToGridVertical(QuestFile file)
            {
                int rowIndex = 0;
                Font font = new Font("Monotype Corsiva", 13);
                font = new Font(font, System.Drawing.FontStyle.Bold); // | System.Drawing.FontStyle.Underline);
                foreach (var row in questFileFieldNamesToPrint)
                {
                    string fieldValue = file.GetType().GetField(row)?.GetValue(file) as string;
                    if (row == "Name")
                    {
                        tbx_questName.Text = fieldValue;
                        continue;
                    }
                    if (row == "LogTopic")
                    {
                        tbx_questLogTopic.Text = fieldValue;
                        continue;
                    }
                    if (row == "Type")
                    {
                        cbx_questType.Text = fieldValue;
                        continue;
                    }
                    if (row == "ExperiencePoints")
                    {
                        tbx_questExperiencePoints.Text = fieldValue;
                        continue;
                    }
                    if (!row.Contains("Amount"))
                    {
                        rowIndex = questGridView.Rows.Add(row);
                        questGridView[1, rowIndex].Value = fieldValue;
                        questGridView[1, rowIndex].ToolTipText = row;
                        questGridView[0, rowIndex].Style.Font = font;
                        questGridView[0, rowIndex].Style.ForeColor = Color.DimGray;
                    }
                    else
                    {
                        questGridView[2, rowIndex].Value = fieldValue;
                        questGridView[2, rowIndex].ToolTipText = row;
                    }

                }
            }

            void FindInfosForQuestFile(QuestFile file)
            {
                if (infoFilesFolder != null)
                {
                    infosWithQuestList.Clear();
                    foreach (var value in dictInfoFiles.Values)
                    {
                        if (value.Quest == file.Name)
                        {
                            infosWithQuestList.Items.Add(value.Name);
                        }
                    }
                }
            }

            PrintFieldsToGridVertical(questfile);
            FindInfosForQuestFile(questfile);
            FindLogTopicTextForQuestFile(questfile);
            
            if (!skipQuestFolderGridReload) { FindQuestsInsideFolder(questfile.Folder, questfile); }
                

        }

        private void FindLogTopicTextForQuestFile(QuestFile questfile)
        {
            if (stringtableIniPath != null)
            {
                if (string.IsNullOrEmpty(questfile.LogTopic)) { return; }

                string[] dialogTexts;

                if (dictStringtable.TryGetValue(questfile.LogTopic, out dialogTexts) == false)
                {
                    dialogTexts = new string[10];
                }
                questfile.TextsByLanguage = dialogTexts;
                questTextBox.Text = dialogTexts[textLanguageList.SelectedIndex];
            }
        }


        private void ActivateInfoFile(string infoName)
        {
            if (infoName.EndsWith("old")) { infoName = infoName.Remove(infoName.Length - 3); }

            if (!dictInfoFiles.ContainsKey(infoName))
            {
                ShowWarning_KeyInDictionaryNotFound(infoName, "dictInfoFiles");
                return;
            }
            InfoFile infofile = currentInfoFile = dictInfoFiles[infoName];
            infofile.wasModified = true;

            infoTextsGrid.Rows.Clear();

            

            void PrintAllFieldsToGrids(InfoFile file)
            {
                foreach (string row in infoFileFieldNamesToPrint)
                {
                    FieldInfo currentField = file.GetType().GetField(row);
                    if (currentField == null) { continue; }

                    string line = null;
                    if (currentField.FieldType == typeof(string[]))
                    {
                        var arrayFromField = currentField.GetValue(file) as string[];    //Gets the actual value of the field as string[]
                        if (arrayFromField == null) { continue; }
                        line = string.Join(";", arrayFromField);
                        if (currentField.Name.StartsWith("InfoScript_"))
                        {
                            PrintFieldsToInfoTextsGrid(currentField.Name, arrayFromField);
                            continue;
                        }
                    }
                    else if (currentField.FieldType == typeof(string))
                    {
                        string fieldValue = currentField.GetValue(file) as string;
                        line = fieldValue;
                    }
                    PrintFieldToGrids(currentField.Name, line);
                    PrintFieldsToPanelControls(currentField.Name, line);

                }
            }

            void PrintFieldsToInfoTextsGrid(string fieldName, string[] fieldValue)
            {
                DataGridViewColumn column = infoTextsGrid.Columns["Col_" + fieldName.Replace("InfoScript_", "")];
                if (column == null) { return; }

                for (int k = 0; k < fieldValue.Length; k++)
                {
                    if (infoTextsGrid.Rows.Count <= k)
                    {
                        int newRowIndex = infoTextsGrid.Rows.Add();
                        infoTextsGrid.Rows[newRowIndex].MinimumHeight = 26;
                    }
                    infoTextsGrid[column.Index, k].Value = fieldValue[k];
                }

                //int i = 0;
                //foreach (var item in fieldValue)
                //{
                //    // i+1 below is only necessary if adding new rows to the grid is enabled. Otherwise it can be just i 
                //    if (infoTextsGrid.Rows.Count <= i)
                //    {
                //        int newRowIndex = infoTextsGrid.Rows.Add();
                //        infoTextsGrid.Rows[newRowIndex].MinimumHeight = 26;
                //    }
                //    infoTextsGrid[column.Index, i].Value = item;
                //    i++;
                //}

                //Color fields in gridview: if npc says dialog - red, if player says dialog - green
                foreach (DataGridViewRow row in infoTextsGrid.Rows)
                {
                    //var commandsCellText = row.Cells[0].Value?.ToString() means (called null conditional):
                    //if value is null, then variable is null; if not, then continue
                    //It's the same as: var commandsCellText = row.Cells[0].Value == null ? null : row.Cells[0].Value.ToString();
                    var commandsCellText = row.Cells[0].Value?.ToString();
                    var entities1CellText = row.Cells[1].Value?.ToString();

                    if (commandsCellText == "Say" && entities1CellText == "npc")
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(231, 177, 177); //Light red
                    }
                    else if (commandsCellText == "Say" && entities1CellText == "player")
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(156, 188, 143); //Light green
                    }
                }

            }

            void PrintFieldToGrids(string fieldName, string value)
            {
                DataGridView[] gridsArray = new DataGridView[] { infoDataGrid1, infoDataGrid2, infoDataGrid3 };

                foreach (DataGridView grid in gridsArray)
                {
                    if (grid.Rows.Count < 1) { grid.Rows.Add(); }
                    DataGridViewColumn column = grid.Columns[fieldName];
                    DataGridViewColumn column1 = grid.Columns["col_" + fieldName];
                    if (column != null)
                    {
                        grid[column.Index, 0].Value = value;
                    }
                    if (column1 != null)
                    {
                        grid[column1.Index, 0].Value = value;
                    }
                }
            }

            void PrintFieldsToPanelControls(string fieldName, string value)
            {
                foreach (var control in infoDataPanel.Controls)
                {
                    if (control is RichTextBox && ((RichTextBox)control).Name.Replace("tbx_info", "") == fieldName)
                    {
                        ((RichTextBox)control).Text = value;
                    }
                    if (control is ComboBox && ((ComboBox)control).Name.Replace("cbx_info", "") == fieldName)
                    {
                        ((ComboBox)control).Text = value;
                    }
                }
            }


            PrintAllFieldsToGrids(infofile);

            
            infoParentsList.Clear();
            FindInfoParents(infofile);
            FindInfoChildren(infofile);
            FindInfosWithSameOwner(infofile);
            AddDialogsWithSameOwnerToGridView(infofile.Owner, infofile: infofile);
            infoFolderButton.Text = infofile.Folder;
            FindOwnersInsideFolder(infofile.Folder, infofile);
            FindQuestForInfoFile(infofile);

            FindDialogsForInfoFile(infofile);
            PrintInfoFileDialogsToGrid(infofile);

        }

        private void FindDialogsForInfoFile(InfoFile infofile)
        {
            if (stringtableIniPath != null)
            {
                foreach (var dialogCode in infofile.InfoScript_Texts)
                {
                    if (string.IsNullOrEmpty(dialogCode)) { continue; }

                    string[] dialogTexts;

                    if (dictStringtable.TryGetValue(dialogCode, out dialogTexts) == false)
                    {
                        dialogTexts = new string[10];
                    }

                    infofile.Dialogs[dialogCode] = dialogTexts;
                    infofile.dialogsLoaded = true; //Putting it here makes sure there is at least 1 dialog
                }
            }
        }

        private void PrintInfoFileDialogsToGrid(InfoFile infofile)
        {
            if (infofile.dialogsLoaded == true)
            {
                for (int i = 0; i < infofile.InfoScript_Texts.Length; i++)
                {
                    string dialogCode = infofile.InfoScript_Texts[i];
                    if (!string.IsNullOrEmpty(dialogCode))
                    {
                        infoTextsGrid[6, i].Value = infofile.Dialogs[dialogCode][textLanguageList.SelectedIndex];
                        //infoTextsGrid[6, i].Value = dictStringtable[dialogCode][textLanguageList.SelectedIndex];
                    }
                }
            }
        }
        #endregion


        #region Quest Controls

        private void questFilesList_ItemActivate(object sender, EventArgs e)
        {
            ActivateQuestFile(questFilesList.SelectedItems[0].Text);
        }


        private void questTextBox_TextChanged(object sender, EventArgs e)
        {
            if (currentQuestFile == null) { return; }
            if (stringtableIniPath == null) { return; }
            currentQuestFile.TextsByLanguage[textLanguageList.SelectedIndex] = questTextBox.Text;
        }

        #endregion


        #region Auxiliary Methods
        private void FindInfoParents(InfoFile infofile)
        {
            if (!String.IsNullOrEmpty(infofile.Parent))
            {
                if (!dictInfoFiles.Keys.Contains(infofile.Parent))
                {
                    ShowWarning_KeyInDictionaryNotFound(infofile.Parent, "dictInfoFiles");
                    return;
                }
                infoParentsList.Items.Add(infofile.Parent);
                FindInfoParents(dictInfoFiles[infofile.Parent]); //should find a parent of the file's parent - but that seems to never happen
            }
        }

        private void FindInfoChildren(InfoFile infofile)
        {
            infoChildrenList.Clear();
            foreach (var value in dictInfoFiles.Values)
            {
                if (value.Parent == infofile.Name)
                {
                    infoChildrenList.Items.Add(value.Name);
                }
            }
        }

        private void FindInfosWithSameOwner(InfoFile infofile)
        {
            infosWithSameOwnerList.Clear();
            foreach (var value in dictInfoFiles.Values)
            {
                if (value.Owner == infofile.Owner)
                {
                    infosWithSameOwnerList.Items.Add(value.Name);
                }
            }
        }

        private void FindAllQuestFolders(QuestFile file)
        {
            List<string> folders = new List<string>();
            foreach (var value in dictQuestFiles.Values)
            {
                if (!folders.Contains(value.Folder))
                {
                    folders.Add(value.Folder);
                }
            }
            //questTextBox.Text = string.Join(Environment.NewLine, folders);
        }


        #endregion


        #region Info Controls
        private void infoFilesList_ItemActivate(object sender, EventArgs e)
        {
            ActivateInfoFile(infoFilesList.SelectedItems[0].Text);
        }
        

        private void FindQuestForInfoFile(InfoFile infofile)
        {
            if (infofile.Quest != "" && findQuestForDialogCheckBox.Checked)
            {
                if (currentQuestFile?.Name != infofile.Quest && !string.IsNullOrEmpty(questFilesFolder))
                {
                    ActivateQuestFile(infofile.Quest);
                }
            }
        }


        

        private void FindQuestsInsideFolder(string folder, QuestFile questfile = null)
        {
            questFolderGrid.Rows.Clear();
            foreach (var value in dictQuestFiles.Values)
            {
                if (value.Folder == folder)
                {
                    int rowIndex = questFolderGrid.Rows.Add(value.Name);
                    if (questfile != null && questfile == value)
                    {
                        questFolderGrid[0, rowIndex].Selected = true;
                    }
                }
            }
        }

        private void FindOwnersInsideFolder(string folder, InfoFile infofile = null)
        {
            infoFolderGrid.Rows.Clear();
            List<string> owners = new List<string>();
            foreach (var value in dictInfoFiles.Values)
            {
                if (value.Folder == folder)
                {
                    if (!owners.Contains(value.Owner))
                    {
                        owners.Add(value.Owner);
                        int rowIndex = infoFolderGrid.Rows.Add(value.Owner);
                        if (infofile != null && infofile.Owner == value.Owner)
                        {
                            infoFolderGrid[0, rowIndex].Selected = true;
                        }
                    }
                }
            }
        }
        
        List<string> filesInGridview = new List<string>();
        private void AddDialogsWithSameOwnerToGridView(string owner, bool forceMethodExecution = false, InfoFile infofile = null)
        {
            List<InfoFile> childrenList = new List<InfoFile>();
            List<InfoFile> parentsList = new List<InfoFile>();

            if (owner != ownerTextbox.Text || forceMethodExecution)
            {
                infoOwnerGrid.Rows.Clear();
                filesInGridview.Clear();
                childrenList.Clear();
                parentsList.Clear();
                FindInfosWithSameOwnerAndDivideIntoParentsAndChildren();
                FindChildrenAndInsertIntoGridview(parentsList, 1);
                currentInfoFileOwner = owner;
                ownerTextbox.Text = owner;
            }

            void FindInfosWithSameOwnerAndDivideIntoParentsAndChildren()
            {
                bool isOwnerFound = false;
                foreach (var value in dictInfoFiles.Values)
                {
                    if (value.Owner == owner)
                    {
                        isOwnerFound = true;
                        FindDialogsForInfoFile(value);
                        if (string.IsNullOrEmpty(value.Parent))     //If this dialog has no parents, set it as the 'original' parent...
                        {
                            parentsList.Add(value);
                        }
                        else                                        //else set add it to the list of children dialogs
                        {
                            childrenList.Add(value);
                        }
                    }
                }
                if (isOwnerFound == false)
                {
                    infoOwnerGrid.Rows.Add("No info files with this owner exist");
                }
                
                parentsList?.Sort(delegate (InfoFile x, InfoFile y)
                {
                    if (x.SortID == null || y.SortID == null) {  return 0; }
                    return x.SortID.CompareTo(y.SortID);
                });
                childrenList?.Sort(delegate (InfoFile x, InfoFile y)
                {
                    if (x.SortID == null || y.SortID == null) { return 0; }
                    return y.SortID.CompareTo(x.SortID);
                });

                foreach (var value in parentsList)
                {
                    int rowIndex = infoOwnerGrid.Rows.Add();
                    infoOwnerGrid[0, rowIndex].ToolTipText = value.Name;
                    infoOwnerGrid[0, rowIndex].Value = (value.dialogsLoaded && showDialogTextsInOwnerListCheckBox.Checked) ?
                                                        value.Dialogs.ElementAt(0).Value[textLanguageList.SelectedIndex] : value.Name;

                    infoOwnerGrid[0, rowIndex].Style.Padding = new Padding(15, 0, 0, 0);
                    filesInGridview.Add(value.Name);
                }

            }
            
            void FindChildrenAndInsertIntoGridview(List<InfoFile> oldParentsList, int iteration)
            {
                if(oldParentsList.Count == 0)
                {
                    return;
                }
                
                List<InfoFile> newParentsList = new List<InfoFile>();
                foreach (var child in childrenList)
                {
                    if (oldParentsList.Any(parent => parent.Name == child.Parent)) 
                    {
                        FindDialogsForInfoFile(child);
                        int index = filesInGridview.IndexOf(child.Parent);
                        string valueToInsert = (child.dialogsLoaded && showDialogTextsInOwnerListCheckBox.Checked) ?
                                                child.Dialogs.ElementAt(0).Value[textLanguageList.SelectedIndex] : child.Name;

                        infoOwnerGrid.Rows.Insert(index + 1, valueToInsert);
                        infoOwnerGrid[0, index + 1].ToolTipText = child.Name;
                        filesInGridview.Insert(index + 1, child.Name);
                        newParentsList.Add(child);
                        //dataGridView1.Rows[index+1].Cells[0].Value = file.Name;
                        infoOwnerGrid[0, index + 1].Style.Padding = new Padding(15 + iteration * 15, 0, 0, 0);
                    }
                }
                iteration++;
                FindChildrenAndInsertIntoGridview(newParentsList, iteration);
            }
            
            if (infofile != null)
            {
                foreach (var file in filesInGridview)
                {
                    if (file == infofile.Name)
                    {
                        int rowIndex = filesInGridview.IndexOf(file);
                        infoOwnerGrid[0, rowIndex].Selected = true;
                        if (showOwnerCondInfoCheckBox.Checked)
                        {
                            InfoOwnerGrid_ShowCondInfo(infoOwnerGrid[0, rowIndex]);
                        }
                        break;
                    }
                }
            }
        }

        private void ownerTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddDialogsWithSameOwnerToGridView(ownerTextbox.Text, true);
                e.SuppressKeyPress = true;
            }
        }


        private void ownerTextbox_Leave(object sender, EventArgs e)
        {
            currentInfoFileOwner = ownerTextbox.Text;
        }


        #endregion

        string InfoScript_Texts_CellValueCopy;
        private void infoTextsGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (infoTextsGrid.Columns[e.ColumnIndex].HeaderText == "Texts")
            {
                InfoScript_Texts_CellValueCopy = infoTextsGrid[e.ColumnIndex, e.RowIndex].Value == null ?
                    "" : infoTextsGrid[e.ColumnIndex, e.RowIndex].Value.ToString();
            }
        }

        private void UpdateDialogText(string dialogCode, string dialogTranslation, int language)
        {
            if (File.Exists(stringtableIniPath))
            {
                if (string.IsNullOrEmpty(dialogCode))
                {
                    string messageTitle = "Null Reference Exception";
                    string messageText =
                        "Most probably you tried to edit a translation text (Dialogs column) in a row that does not display a dialog " +
                        "(or there is nothing in the Texts column).";
                    System.Windows.Forms.MessageBox.Show(messageText, messageTitle);
                    return;
                }

                if (!currentInfoFile.Dialogs.ContainsKey(dialogCode))
                {
                    currentInfoFile.Dialogs[dialogCode] = new string[10];
                }
                currentInfoFile.Dialogs[dialogCode][language] = dialogTranslation;

            }
            else
            {
                ShowWarning_StringtableIniNotFound();
                return;
            }
        }

        private void DeleteInfofileDialog(string dialogCode)
        {
            if (!string.IsNullOrEmpty(dialogCode) && stringtableIniPath != null)
            {
                bool infoScriptText_FoundInAnotherRow = false;
                foreach (DataGridViewRow row in infoTextsGrid.Rows)
                {
                    if (infoTextsGrid[5, row.Index].Value == null)
                        continue;

                    if (infoTextsGrid[5, row.Index].Value.ToString() == dialogCode)
                        infoScriptText_FoundInAnotherRow = true;
                }

                if (infoScriptText_FoundInAnotherRow == false)
                {
                    if (currentInfoFile.Dialogs.ContainsKey(dialogCode))
                    {
                        currentInfoFile.Dialogs.Remove(dialogCode);
                    }
                    else
                    {
                        ShowWarning_KeyInDictionaryNotFound(dialogCode, currentInfoFile.Name + ".Dialogs");
                        //Something went very wrong, as at this stage the key definitely should be in the dictionary
                    }
                }

                bool infoScriptText_FoundInAnotherFile = false;
                foreach (InfoFile infofile in dictInfoFiles.Values)
                {
                    foreach (string textCode in infofile.InfoScript_Texts)
                    {
                        if (textCode == dialogCode) { infoScriptText_FoundInAnotherFile = true; }
                    }
                }

                if (infoScriptText_FoundInAnotherFile == false)
                {
                    if (dictStringtable.ContainsKey(dialogCode))
                    {
                        dictStringtable.Remove(dialogCode);
                    }
                    else
                    {
                        ShowWarning_KeyInDictionaryNotFound(dialogCode, currentInfoFile.Name + ".Dialogs");
                        //Something went very wrong, as at this stage the key definitely should be in the dictionary
                    }
                }

            }
        }

        private void infoTextsGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var field = currentInfoFile.GetType().GetField("InfoScript_" + infoTextsGrid.Columns[e.ColumnIndex].HeaderText);
            string cellText = infoTextsGrid.CurrentCell.Value == null ?
                "" : infoTextsGrid.CurrentCell.Value.ToString();
            
            if (field != null && field.Name != "InfoScript_Dialogs")
            {
                if (field.Name == "InfoScript_Texts" && stringtableIniPath != null)
                {
                    if (InfoScript_Texts_CellValueCopy != cellText) //-> is previous cell value different than the end edit value
                    {
                        if (currentInfoFile.Dialogs.ContainsKey(cellText))
                        {
                            System.Windows.Forms.MessageBox.Show("Key already exists!");
                            infoTextsGrid.CurrentCell.Value = InfoScript_Texts_CellValueCopy;
                            return;
                        }

                        bool infoScriptText_FoundInAnotherRow = false;  //-> is the same textCode present in another row
                        foreach (DataGridViewRow row in infoTextsGrid.Rows)
                        {
                            if (infoTextsGrid[e.ColumnIndex, row.Index].Value == null)
                                continue;

                            if (infoTextsGrid[e.ColumnIndex, row.Index].Value.ToString() == InfoScript_Texts_CellValueCopy)
                                infoScriptText_FoundInAnotherRow = true;
                        }

                        if (string.IsNullOrEmpty(cellText))
                        {
                            if (!infoScriptText_FoundInAnotherRow)  //PROBLEM: textCode can still be present in another info file!
                            {
                                DeleteInfofileDialog(InfoScript_Texts_CellValueCopy);
                            } 
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(InfoScript_Texts_CellValueCopy) || infoScriptText_FoundInAnotherRow)
                            {
                                currentInfoFile.Dialogs.Add(cellText, new string[10]);
                            }
                            else
                            {
                                currentInfoFile.Dialogs.ChangeKey(InfoScript_Texts_CellValueCopy, cellText);
                            }
                        }
                        

                        string dialogs_CellText = infoTextsGrid[6, e.RowIndex].Value == null ?
                            "" : infoTextsGrid[6, e.RowIndex].Value.ToString();

                        UpdateDialogText(cellText, dialogs_CellText, textLanguageList.SelectedIndex);
                    }

                    //currentInfoFile.dialogsLoaded = false;
                    //FindDialogsForInfoFile(currentInfoFile);
                }
                var arrayFromField = field.GetValue(currentInfoFile) as string[];
                arrayFromField[e.RowIndex] = cellText;
                field.SetValue(currentInfoFile, arrayFromField);
            }
            else //cell.ColumnIndex = 6 (Dialogs column)
            {
                string infoScriptTexts_CellText = infoTextsGrid[5, e.RowIndex].Value == null ?
                    "" : infoTextsGrid[5, e.RowIndex].Value.ToString();

                UpdateDialogText(infoScriptTexts_CellText, cellText, textLanguageList.SelectedIndex);
            }
        }

        private void allInfoGrids_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var gridView = sender as DataGridView;
            var field = currentInfoFile.GetType().GetField(gridView.Columns[e.ColumnIndex].HeaderText);
            if (field != null)
            {
                string fieldValue = gridView[e.ColumnIndex, e.RowIndex].Value?.ToString(); //cellText
                field.SetValue(currentInfoFile, fieldValue);
            }
        }


        private void questGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var gridView = sender as DataGridView;
            var field = currentQuestFile.GetType().GetField(gridView[e.ColumnIndex, e.RowIndex].ToolTipText);
            if (field != null)
            {
                string fieldValue = gridView[e.ColumnIndex, e.RowIndex].Value?.ToString(); //cellText
                field.SetValue(currentQuestFile, fieldValue);
            }
        }

        #region Warning Messages
        private void ShowWarning_StringtableIniNotFound()
        {
            string messageTitle = "Choose stringtable.ini file path first!";
            string messageText = "You are trying to change a translation text, " +
                "but stringtable.ini file path doesn't exist or has not been chosen. " +
                "Choose the stringtable.ini file path first!" +
                "(In the upper left corner of the app there is a button to chooose the file)";

            System.Windows.Forms.MessageBox.Show(messageText, messageTitle);
        }

        private void ShowWarning_FileNotFound(string filePath)
        {
            string methodName = new StackFrame(1).GetMethod().Name; //This shows the name of the method that called this
                                                                   //StackFrame(0) means the method that called this code
                                                                  //StackFrame(1) means 1 method 'higher' - the method that called this method, etc.
            string messageTitle = "File not found!";
            string messageText = "Program couldn't find the following file: \n" + filePath + "\n"
                + "The error occured while the following method was running: \n" + methodName;
            System.Windows.Forms.MessageBox.Show(messageText, messageTitle);
        }

        private void ShowWarning_FileIsProbablyEmpty()
        {
            string methodName = new StackFrame(1).GetMethod().Name; //This shows the name of the method that called this
                                                                    //StackFrame(0) means the method that called this code
                                                                    //StackFrame(1) means 1 method 'higher' - the method that called this method, etc.
            string messageTitle = "File Is Probably Empty!";
            string messageText = "There was a problem with the file you tried to open" + "\n"
                + "The error occured while the following method was running: \n" + methodName + "\n"
                + "The whole file is probably empty or there is some other serious issue with it.\n"
                + "Contents (or at least some of them) of this file have probably been loaded into the program, but there may be some problems\n"
                + "To be sure that everything is fine, check the file, modify or delete it if necessary and reload the program";
            System.Windows.Forms.MessageBox.Show(messageText, messageTitle);
        }

        private void ShowWarning_FileIsProbablyEmpty(string filePath)
        {
            string methodName = new StackFrame(1).GetMethod().Name; //This shows the name of the method that called this
                                                                    //StackFrame(0) means the method that called this code
                                                                    //StackFrame(1) means 1 method 'higher' - the method that called this method, etc.
            string messageTitle = "File Is Probably Empty!";
            string messageText = "There was a problem while reading the following file: \n" + filePath + "\n"
                + "The error occured while the following method was running: \n" + methodName + "\n"
                + "The 'Name' field in the file was empty, which suggests that the whole file is probably empty or that there is some other serious issue with it.\n"
                + "Contents of this file have not been loaded into the program, although the file will appear in the files list!";
            System.Windows.Forms.MessageBox.Show(messageText, messageTitle);
        }

        private void ShowWarning_KeyInDictionaryNotFound(string key, string dictionary)
        {
            string messageTitle = "Key in dictionary not found!";
            string messageText = "The following key: \n" + key + "\n" +
                "Was not found in the following dictionary: \n" + dictionary + "\n" +
                "NOTE: you should definitely not click on a button if it says 'No info files with this owner exist'";
            System.Windows.Forms.MessageBox.Show(messageText, messageTitle);
        }

        private void ShowWarning_TranslationNotFound()
        {
            string messageTitle = "Translation not found!";
            string messageText = "Translations for the current .info file could not be found. \n" +
                "Try reloading the current info file by double-clicking its name on the info files list. \n" +
                "If that doesn't help, make sure you have selected the correct path to the stringtable.ini file.";
            System.Windows.Forms.MessageBox.Show(messageText, messageTitle);
        }
        #endregion

       
        private void infoOrQuestFilesList_MouseClick(object sender, MouseEventArgs e)
        {
            ListView snd = sender as ListView;
            if (e.Button == MouseButtons.Right)
            {
                if (snd.FocusedItem != null && snd.FocusedItem.Bounds.Contains(e.Location))
                {
                    infoOrQuestFilesContextMenuStrip.Show(Cursor.Position);
                }
            }
        }

        private void infoOrQuestFilesContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string path;
            string itemText;
            ListViewItem focusedItem;
            ListView listview;
            if (infoFilesList.Focused)
            {
                focusedItem = infoFilesList.FocusedItem;
                itemText = infoFilesList.FocusedItem.Text;
                listview = infoFilesList;
                if (itemText.EndsWith("old")) { path = Path.Combine(infoFilesFolder, itemText.Remove(itemText.Length - 3) + infoFileExtension + "old"); }
                else { path = Path.Combine(infoFilesFolder, itemText + infoFileExtension); }
            }
            else if (infoOwnerGrid.Focused)
            {
                focusedItem = null;
                itemText = infoOwnerGrid.CurrentCell.ToolTipText;
                listview = infoFilesList;
                if (itemText.EndsWith("old")) { path = Path.Combine(infoFilesFolder, itemText.Remove(itemText.Length - 3) + infoFileExtension + "old"); }
                else { path = Path.Combine(infoFilesFolder, itemText + infoFileExtension); }
            }
            else if (questFilesList.Focused)
            {
                focusedItem = questFilesList.FocusedItem;
                itemText = questFilesList.FocusedItem.Text;
                listview = questFilesList;
                if (itemText.EndsWith("old")) { path = Path.Combine(questFilesFolder, itemText.Remove(itemText.Length - 3) + questFileExtension + "old"); }
                else { path = Path.Combine(questFilesFolder, itemText + questFileExtension); }
            }
            else
            { 
                return; 
            } 


            if (File.Exists(path))
            {
                if (e.ClickedItem == infoOrQuestFilesContextMenuStrip.Items["itemOpenFolder"])
                {
                    //This opens a separate explorer window even if one with the path is already opened
                    //But it does open the window with the clicked file selected
                    Process.Start("explorer.exe", "/select, " + path);

                    //This does not open a separate explorer window if one with the path is already opened
                    //But it does not select the file in the opened folder
                    //Process.Start(infoFilesFolder);

                }
                else if (e.ClickedItem == infoOrQuestFilesContextMenuStrip.Items["itemOpenFile"])
                {
                    Process.Start(path);
                }

                else if (e.ClickedItem == infoOrQuestFilesContextMenuStrip.Items["itemDeleteFile"])
                {
                    if (System.Windows.Forms.MessageBox.Show("Are you sure that you want to delete the following file from your computer: " + path, "Delete File?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        File.Delete(path);
                        listview.FocusedItem.Remove();
                    }
                }
                else if (e.ClickedItem == infoOrQuestFilesContextMenuStrip.Items["itemRefreshList"])
                {
                    if (infoFilesList.Focused)
                    {
                        UpdateQuestOrInfoFilesList(infoFilesFolder, infoFilesList);
                    }
                    else if (infoOwnerGrid.Focused)
                    {
                        AddDialogsWithSameOwnerToGridView(ownerTextbox.Text, true);
                    }
                    else if (questFilesList.Focused)
                    {
                        UpdateQuestOrInfoFilesList(questFilesFolder, questFilesList);
                    }
                }
            }
        }


        private void infosWithQuestList_MouseClick(object sender, MouseEventArgs e)
        {
            var focusedItem = infosWithQuestList.FocusedItem;
            if (focusedItem != null && focusedItem.Bounds.Contains(e.Location))
            {
                var item = infoFilesList.FindItemWithText(focusedItem.Text, false, 0, false);
                item.Selected = true;
                infoFilesList.EnsureVisible(item.Index);
            }
        }


        //e.g.BPANKRATZ314 and BPANKRATZ327 both set Umbrak_Offer (BPANKRATZ336 has it as CondPlayerKnows)
        private void FindInfosThatSetTheSameEvent()
        {
            Dictionary<string, List<string>> infosThatSetTheSameEvent = new Dictionary<string, List<string>>();
            foreach (var value in dictInfoFiles.Values)
            {
                string[] commands1 = value.InfoScript_Commands;
                string[] ids1_1 = value.InfoScript_IDs1;
                for (int m = 0; m < commands1.Length; m++)
                {
                    if (commands1[m] == "SetGameEvent")
                    {
                        if (!infosThatSetTheSameEvent.Keys.Contains(ids1_1[m]))
                        {
                            infosThatSetTheSameEvent.Add(ids1_1[m], new List<string> { value.Name });
                        }
                        else
                        {
                            infosThatSetTheSameEvent[ids1_1[m]].Add(value.Name);
                        }

                    }
                }
            }
            foreach (var info in infosThatSetTheSameEvent)
            {
                if (info.Value.Count > 1)
                {
                    infosWithQuestList.Items.Add(info.Key.ToString());
                    foreach (var infoName in info.Value)
                    {
                        infosWithQuestList.Items.Add(infoName);
                    }
                }
            }
        }

        private void FindInfosThatSetMultipleEvents()
        {
            List<string> infosWithMultipleSetEvents = new List<string>();
            foreach (var value in dictInfoFiles.Values)
            {
                string[] commands1 = value.InfoScript_Commands;
                string[] ids1_1 = value.InfoScript_IDs1;
                List<string> events1 = new List<string>();
                for (int m = 0; m < commands1.Length; m++)
                {
                    if (commands1[m] == "SetGameEvent")
                    {
                        events1.Add(ids1_1[m]);
                    }
                }
                if (events1.Count > 1)
                {
                    infosWithMultipleSetEvents.Add(value.Name);
                }
            }
        }

        private Button CreateEventNameButton(string text)
        {
            var parentContainer = infoOwnerGrid.Parent;
            Button btn = new Button();
            btn.Size = new System.Drawing.Size(160, 26);
            btn.Text = text;
            btn.Enabled = false;
            parentContainer.Controls.Add(btn);
            buttons.Add(btn);
            return btn;
        }

        private Button CreateEventDialogsButton(InfoFile value, Color backColor)
        {
            var parentContainer = infoOwnerGrid.Parent;
            Button btn = new Button();
            btn.Text = (value.dialogsLoaded && showDialogTextsInOwnerListCheckBox.Checked) ?
                        value.Dialogs.ElementAt(0).Value[textLanguageList.SelectedIndex] : value.Name;
            btn.Size = new System.Drawing.Size(200, 26);
            btn.BackColor = backColor;
            foreach (DataGridViewRow row in infoOwnerGrid.Rows)
            {
                if (infoOwnerGrid[0, row.Index].ToolTipText == value.Name)
                {
                    row.DefaultCellStyle.BackColor = backColor;
                    //infoOwnerGrid[0, row.Index].Style.BackColor = Color.FromArgb(231, 177, 177);
                    //infoOwnerGrid[0, row.Index].Style.SelectionBackColor = Color.FromArgb(231, 177, 177);
                }
            }

            ToolTip toolTip1 = new ToolTip();
            toolTip1.SetToolTip(btn, value.Name);
            parentContainer.Controls.Add(btn);
            buttons.Add(btn);
            btn.Click += Button_Click;
            void Button_Click(object sender, EventArgs e)
            {
                ActivateInfoFile(value.Name);
            }
            return btn;
        }
        
        
        private void InfoOwnerGrid_ShowCondInfo(DataGridViewCell currentCell)
        {
            var condInfoName = dictInfoFiles[currentCell.ToolTipText.ToString()].CondPlayerKnows;
            var cellRectangle = infoOwnerGrid.GetCellDisplayRectangle(currentCell.ColumnIndex, currentCell.RowIndex, true);
            System.Drawing.Point loc = infoOwnerGrid.PointToScreen(cellRectangle.Location);
            System.Drawing.Point finalLoc = PointToClient(loc);

            ShowConditionEventsForThisDialog();
            ShowSetGameEventsForThisDialog();

            //Show files that set the game event which is the CondPlayerKnows event for this dialog
            void ShowConditionEventsForThisDialog()
            {
                if (condInfoName == null) { return; }

                if (condInfoName != "")
                {
                    int iterationIndex = 0;
                    foreach (var value in dictInfoFiles.Values)
                    {
                        if (value.InfoScript_Commands.Contains("SetGameEvent") && value.InfoScript_IDs1.Any(a => a == condInfoName))
                        {
                            Button btnCondName = CreateEventNameButton(condInfoName);
                            btnCondName.Location = new System.Drawing.Point(finalLoc.X - btnCondName.Size.Width - 20, finalLoc.Y);

                            FindDialogsForInfoFile(dictInfoFiles[value.Name]);
                            Button btn = CreateEventDialogsButton(value, Color.FromArgb(231, 177, 177));
                            btn.Location = new System.Drawing.Point(finalLoc.X - btn.Size.Width,
                                                                     finalLoc.Y + btnCondName.Size.Height + iterationIndex * btn.Size.Height);

                            iterationIndex++;
                        }
                    }
                }
                
            }
            

            //Show which game events this dialog sets
            void ShowSetGameEventsForThisDialog()
            {
                List<string> events = new List<string>();
                string[] commands = currentInfoFile.InfoScript_Commands;
                string[] ids1 = currentInfoFile.InfoScript_IDs1;
                if (commands == null || ids1 == null) { return; }
                
                int j = 0;
                for (int i = 0; i < commands.Length; i++)
                {
                    if (commands[i] == "SetGameEvent")
                    {
                        events.Add(ids1[i]);
                        Button buttonEventName = CreateEventNameButton(ids1[i]);
                        buttonEventName.Location = new System.Drawing.Point(finalLoc.X + currentCell.Size.Width + 30, finalLoc.Y + j * 26);

                        foreach (var value in dictInfoFiles.Values)
                        {
                            if (value.CondPlayerKnows == ids1[i])
                            {
                                FindDialogsForInfoFile(dictInfoFiles[value.Name]);
                                Button btn = CreateEventDialogsButton(value, Color.FromArgb(156, 188, 143));
                                btn.Location = new System.Drawing.Point(finalLoc.X + currentCell.Size.Width + 10,
                                                                         finalLoc.Y + buttonEventName.Size.Height + j * btn.Size.Height);
                                j++;
                            }
                        }
                        j++;
                    }
                }
            }
            
        }


        private void questFolderGrid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewCell currentCell;
            if (e.RowIndex >= 0 && e.RowIndex < questFolderGrid.Rows.Count &&
                    e.ColumnIndex >= 0 && e.ColumnIndex < questFolderGrid.Columns.Count)
            {
                currentCell = questFolderGrid[e.ColumnIndex, e.RowIndex];
                questFolderGrid.CurrentCell = currentCell;
            }
            else
            {
                currentCell = null;
            }

            if (e.Button == MouseButtons.Left && currentCell != null)
            {
                ActivateQuestFile(currentCell.Value.ToString(), skipQuestFolderGridReload: true);
            }
        }

        private void infoFolderGrid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewCell currentCell;
            if (e.RowIndex >= 0 && e.RowIndex < infoFolderGrid.Rows.Count &&
                    e.ColumnIndex >= 0 && e.ColumnIndex < infoFolderGrid.Columns.Count)
            {
                currentCell = infoFolderGrid[e.ColumnIndex, e.RowIndex];
                infoFolderGrid.CurrentCell = currentCell;
            }
            else
            {
                currentCell = null;
            }

            if (e.Button == MouseButtons.Left && currentCell != null)
            {
                //System.Windows.Forms.MessageBox.Show(currentCell.Value.ToString());
                AddDialogsWithSameOwnerToGridView(currentCell.Value.ToString(), true);
            }
        }


        private void infoOwnerGrid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewCell currentCell;
            if (e.RowIndex >= 0 && e.RowIndex < infoOwnerGrid.Rows.Count &&
                    e.ColumnIndex >= 0 && e.ColumnIndex < infoOwnerGrid.Columns.Count)
            {
                currentCell = infoOwnerGrid[e.ColumnIndex, e.RowIndex];
                infoOwnerGrid.CurrentCell = currentCell;
            }
            else
            {
                currentCell = null;
            }

            ClearButtons();

            if (e.Button == MouseButtons.Left && currentCell != null)
            {
                ActivateInfoFile(currentCell.ToolTipText);
            }
                
            if (e.Button == MouseButtons.Right)
            {
                infoOwnerGrid.Focus();
                
                if (currentCell != null && currentCell.ContentBounds.Contains(e.Location))
                {
                    //infoFilesContextMenuStrip.Text = string.Format("infoOwnerGrid[{0}, {1}]", e.ColumnIndex, e.RowIndex); //"infoOwnerGrid[" + e.ColumnIndex + ", " + e.RowIndex 
                    infoOrQuestFilesContextMenuStrip.Show(Cursor.Position);
                    //System.Windows.Forms.MessageBox.Show(infoFilesContextMenuStrip.Text);
                }
            }
        }

        List<Button> buttons = new List<Button>();
        private void ClearButtons()
        {
            var parentContainer = infoOwnerGrid.Parent;
            foreach (Button button in buttons)
            {
                parentContainer.Controls.Remove(button);
            }
            buttons.Clear();

            foreach (DataGridViewRow row in infoOwnerGrid.Rows)
            {
                row.DefaultCellStyle.BackColor = Color.White;
                //infoOwnerGrid[0, row.Index].Style.BackColor = Color.FromArgb(231, 177, 177);
                //infoOwnerGrid[0, row.Index].Style.SelectionBackColor = Color.FromArgb(231, 177, 177);
            }
        }
        
        private void infoOwnerGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            ClearButtons();
        }

        private void infoFileTextsGrid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewCell currentCell;
            if (e.RowIndex >= 0 && e.RowIndex < infoTextsGrid.Rows.Count &&
                    e.ColumnIndex >= 0 && e.ColumnIndex < infoTextsGrid.Columns.Count)
            {
                currentCell = infoTextsGrid[e.ColumnIndex, e.RowIndex];
                infoTextsGrid.CurrentCell = currentCell;
            }
            else
            {
                return;
            }

            foreach (ToolStripMenuItem item in commandsMenuStrip.Items)
            {
                item.DropDownItemClicked += CommandsContextMenuStrip_ItemClicked;
            }


            if (e.Button == MouseButtons.Left && currentCell != null && currentCell.ColumnIndex == 0)
            {
                commandsMenuStrip.Show(Cursor.Position);
            }
            
            //WHY IS IT AFTER CLICKING ON THE GRID??
            if (currentCell.ColumnIndex == 3 && infoTextsGrid[0, currentCell.RowIndex].Value?.ToString() == "SetGameEvent")
            {
                infosWithQuestList.Items.Clear();
                foreach (var value in dictInfoFiles.Values)
                {
                    if (value.CondPlayerKnows == currentCell.Value.ToString())
                    {
                        infosWithQuestList.Items.Add(value.Name);
                    }

                }
            }

            //commandsContextMenuStrip.ItemClicked += CommandsContextMenuStrip_ItemClicked;
            //commandsMenuStrip.MouseWheel += (ob, ev) => SendKeys.SendWait(ev.Delta > 0 ? "{UP}" : "{DOWN}");
        }

        private void CommandsContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            infoTextsGrid.CurrentCell.Value = e.ClickedItem.Text;

            var field = currentInfoFile.GetType().GetField("InfoScript_Commands");
            var arrayFromField = field.GetValue(currentInfoFile) as string[];
            arrayFromField[infoTextsGrid.CurrentCell.RowIndex] = infoTextsGrid.CurrentCell.Value.ToString(); //cellText
            field.SetValue(currentInfoFile, arrayFromField);
        }

        //Taken from: https://stackoverflow.com/questions/40278350/change-color-of-button-in-datagridview#
        //Allows to paint Buttons inside GridView to specific color, keeping its FlatStyle as Standard, instead of Popup
        //Generally looks better, but the following method needs to be adjusted
        private void infoOwnerGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            //if (e.RowIndex < 0 || e.ColumnIndex < 0)
            //    return;
            //if (e.ColumnIndex == 0) // Also you can check for specific row by e.RowIndex
            //{
            //    e.Paint(e.CellBounds, DataGridViewPaintParts.All
            //        & ~(DataGridViewPaintParts.ContentForeground));
            //    var r = e.CellBounds;
            //    r.Inflate(-4, -4);
            //    e.Graphics.FillRectangle(Brushes.BurlyWood, r);
            //    e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);
            //    e.Handled = true;
            //}
        }

        private void questFolderButton_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem item in questFolderMenuStrip.Items)
            {
                item.DropDownItemClicked += QuestFolderContextMenuStrip_ItemClicked;
            }
            questFolderMenuStrip.Show(Cursor.Position);
        }

        private void QuestFolderContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem parentToolStrip = e.ClickedItem.OwnerItem;
            questFolderButton.Text = e.ClickedItem.Text == "..." ? parentToolStrip.Text + "/" :
                                                                  parentToolStrip.Text + "/" + e.ClickedItem.Text + "/";

            FindQuestsInsideFolder(questFolderButton.Text);
        }

        private void infoFolderButton_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem item in infoFolderMenuStrip.Items)
            {
                item.DropDownItemClicked += InfoFolderContextMenuStrip_ItemClicked;
            }
            infoFolderMenuStrip.Show(Cursor.Position);
        }
        
        private void InfoFolderContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripItem parentToolStrip = e.ClickedItem.OwnerItem;
            if (e.ClickedItem.Text == "...") {
                infoFolderButton.Text = parentToolStrip.Text + "/";
            }
            else {
                infoFolderButton.Text = parentToolStrip.Text + "/" + e.ClickedItem.Text + "/";
            }
            FindOwnersInsideFolder(infoFolderButton.Text);
        }

        private void dialogCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox snd = sender as CheckBox;
            if (snd.Checked) //This is the state AFTER CheckedChanged event
            {
                snd.Image = imageList1.Images[3];
            }
            else
            {     
                snd.Image = imageList1.Images[2];
            }
        }

        private void allInfoGrids_Paint(object sender, PaintEventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                SizeF size = TextRenderer.MeasureText(col.HeaderText, grid.ColumnHeadersDefaultCellStyle.Font);
                if (size.Width > col.Width)
                {
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else
                {
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                }
            }
            
        }

        
        private void createNewFileButton_Click(object sender, EventArgs e)
        {
            //int promptValue = Prompt.ShowDialog("Test", "Create New File");
            FormCreateNewFile createNewFile = new FormCreateNewFile();
            createNewFile.btn_NewFileConfirm.Click += btn_NewFileConfirm_Click;
            createNewFile.cbx_NewFileExtension.Items.AddRange(new string[] { questFileExtension, infoFileExtension });
            createNewFile.cbx_NewFileExtension.SelectedIndex = 0;

            createNewFile.StartPosition = FormStartPosition.CenterParent;
            createNewFile.ShowDialog();


            //Form formBackground = new Form();
            //using (createNewFile)
            //{

            //    formBackground.StartPosition = FormStartPosition.Manual;
            //    formBackground.FormBorderStyle = FormBorderStyle.None;
            //    formBackground.Opacity = 0.2d;
            //    formBackground.BackColor = Color.Black;
            //    formBackground.WindowState = FormWindowState.Maximized;
            //    formBackground.TopMost = false;
            //    formBackground.Bounds = this.Bounds;
            //    formBackground.ShowInTaskbar = false;
            //    formBackground.Show(this);

            //    //createNewFile.Owner = formBackground;
            //    createNewFile.ShowDialog(this);

            //    formBackground.Dispose();
            //}

        }


        private void CreateNewQuestOrInfoFile(string fileName, string fileExtension)
        {
            if (string.IsNullOrEmpty(fileName)) { ShowWarning_FileIsProbablyEmpty(fileName); return; }

            string filepath; string firstLine; ListView listview; QuestOrInfoFile newFile;

            if (fileExtension == questFileExtension)
            {
                if (questFilesFolder == null) { ShowWarning_StringtableIniNotFound(); return; }
                firstLine = "[Quest]";
                listview = questFilesList;
                filepath = Path.Combine(questFilesFolder, fileName + fileExtension);
                newFile = new QuestFile();
            }
            else
            {
                if (infoFilesFolder == null) { ShowWarning_StringtableIniNotFound(); return; }
                firstLine = "[Info]";
                listview = infoFilesList;
                filepath = Path.Combine(infoFilesFolder, fileName + fileExtension);
                newFile = new InfoFile();
            }

            newFile.Name = fileName;
            newFile.FileName = fileName + fileExtension;
            newFile.FilePath = filepath;
            PrintValueFromFields(newFile);
            AddOrUpdateFilesDict(fileName, newFile);
            listview.Items.Add(fileName);

            //using (StreamWriter writer = new StreamWriter(filepath))
            //{
            //    writer.WriteLine(firstLine);
            //    writer.WriteLine("Name=" + fileName);
            //}
            //LoadQuestOrInfoToDict(filepath);
            //listview.Items.Add(fileName);

            if (fileExtension == questFileExtension)    { ActivateQuestFile(fileName); }
            else                                        { ActivateInfoFile(fileName); }
        }

        public void btn_NewFileConfirm_Click(object sender, EventArgs e)
        {
            FormCreateNewFile currentNewFileForm = (sender as Button).Parent as FormCreateNewFile;
            string fileName = currentNewFileForm.tbx_NewFileName.Text;
            string fileExtension = currentNewFileForm.cbx_NewFileExtension.Text;

            if (string.IsNullOrEmpty(fileName)) { ShowWarning_FileIsProbablyEmpty(fileName); return; }

            CreateNewQuestOrInfoFile(fileName, fileExtension);

            currentNewFileForm.Dispose();
        }

        string[] infoTextsGrid_RowCellsCopy = new string[7];
        
        private void infoTextsGrid_contextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (infoTextsGrid.CurrentCell == null)
            {
                System.Windows.Forms.MessageBox.Show("Current cell is null");
                return;
            }

            if (e.ClickedItem == infoTextsGrid_contextMenuItem_InsertRowAbove)
            {
                infoTextsGrid.Rows.Insert(infoTextsGrid.CurrentCell.RowIndex);
                infoTextsGrid_UpdateInfofileFields();
            }
            else if (e.ClickedItem == infoTextsGrid_contextMenuItem_InsertRowBelow)
            {
                infoTextsGrid.Rows.Insert(infoTextsGrid.CurrentCell.RowIndex + 1);
                infoTextsGrid_UpdateInfofileFields();
            }
            else if (e.ClickedItem == infoTextsGrid_contextMenuItem_CopyRow)
            {
                for (int i = 0; i < infoTextsGrid.ColumnCount; i++)
                {
                    infoTextsGrid_RowCellsCopy[i] = infoTextsGrid[i, infoTextsGrid.CurrentCell.RowIndex].Value == null ?
                        "" : infoTextsGrid[i, infoTextsGrid.CurrentCell.RowIndex].Value.ToString();
                }
            }
            else if (e.ClickedItem == infoTextsGrid_contextMenuItem_PasteRow)
            {
                string infoScriptText_CellText = infoTextsGrid[5, infoTextsGrid.CurrentCell.RowIndex].Value == null ?
                    "" : infoTextsGrid[5, infoTextsGrid.CurrentCell.RowIndex].Value.ToString();
                string dialogs_CellText = infoTextsGrid[6, infoTextsGrid.CurrentCell.RowIndex].Value == null ?
                    "" : infoTextsGrid[6, infoTextsGrid.CurrentCell.RowIndex].Value.ToString();

                for (int i = 0; i < infoTextsGrid.ColumnCount; i++)
                {
                    infoTextsGrid[i, infoTextsGrid.CurrentCell.RowIndex].Value = infoTextsGrid_RowCellsCopy[i];
                }

                string infoScriptText_NewCellText = infoTextsGrid[5, infoTextsGrid.CurrentCell.RowIndex].Value == null ?
                    "" : infoTextsGrid[5, infoTextsGrid.CurrentCell.RowIndex].Value.ToString();
                string dialogs_NewCellText = infoTextsGrid[6, infoTextsGrid.CurrentCell.RowIndex].Value == null ?
                    "" : infoTextsGrid[6, infoTextsGrid.CurrentCell.RowIndex].Value.ToString();


                infoTextsGrid_UpdateInfofileFields();
                UpdateDialogText(infoScriptText_NewCellText, dialogs_NewCellText, textLanguageList.SelectedIndex);
                if (infoScriptText_NewCellText != infoScriptText_CellText)
                {
                    DeleteInfofileDialog(infoScriptText_CellText);
                }
            }
            else if (e.ClickedItem == infoTextsGrid_contextMenuItem_DeleteRow)
            {
                int rowIndex = infoTextsGrid.CurrentCell.RowIndex;
                string infoScriptText_CellText = infoTextsGrid[5, rowIndex].Value == null ?
                    "" : infoTextsGrid[5, rowIndex].Value.ToString();

                infoTextsGrid.Rows.RemoveAt(rowIndex);
                infoTextsGrid_UpdateInfofileFields();
                DeleteInfofileDialog(infoScriptText_CellText);
            }
        }

        
        private void infoTextsGrid_UpdateInfofileFields()
        {
            foreach (DataGridViewColumn column in infoTextsGrid.Columns)
            {
                var field = currentInfoFile.GetType().GetField("InfoScript_" + column.HeaderText);

                if (field != null)
                {
                    List<string> newList = new List<string>();
                    foreach (DataGridViewRow row in infoTextsGrid.Rows)
                    {
                        string text = infoTextsGrid[column.Index, row.Index].Value == null ?
                            "" : infoTextsGrid[column.Index, row.Index].Value.ToString();

                        newList.Add(text);
                    }
                    field.SetValue(currentInfoFile, newList.ToArray());
                }
            }
        }


        #region Resizing the Form
        private void ResizeFormToAll()
        {
            ActiveForm.Size = new System.Drawing.Size(infosPanel.Width + 30 + questsPanel.Width, ActiveForm.Size.Height);

            infosPanel.Location = new System.Drawing.Point(-5, infosPanel.Location.Y);
            questsPanel.Location = new System.Drawing.Point(ActiveForm.Width - 10 - questsPanel.Width, questsPanel.Location.Y);

            if (!IsPanelFullyVisible(questsPanel) || !IsPanelFullyVisible(infosPanel))
            {
                resizeToInfosPictureBox.Image = Properties.Resources.right_arrow;
                resizeToQuestsPictureBox.Image = Properties.Resources.left_arrow;
            }
            else
            {
                resizeToInfosPictureBox.Image = Properties.Resources.left_arrow;
                resizeToQuestsPictureBox.Image = Properties.Resources.right_arrow;
            }
        }

        private void ResizeFormToQuests()
        {
            ActiveForm.Size = new System.Drawing.Size(questsPanel.Width, ActiveForm.Size.Height);

            infosPanel.Location = new System.Drawing.Point(-infosPanel.Width - 10, infosPanel.Location.Y);
            questsPanel.Location = new System.Drawing.Point(0, questsPanel.Location.Y);

            if (!IsPanelFullyVisible(questsPanel) || !IsPanelFullyVisible(infosPanel))
            {
                resizeToInfosPictureBox.Image = Properties.Resources.right_arrow;
                resizeToQuestsPictureBox.Image = Properties.Resources.left_arrow;
            }
            else
            {
                resizeToInfosPictureBox.Image = Properties.Resources.left_arrow;
                resizeToQuestsPictureBox.Image = Properties.Resources.right_arrow;
            }
        }

        private void ResizeFormToInfos()
        {
            ActiveForm.Size = new System.Drawing.Size(infosPanel.Width, ActiveForm.Size.Height);

            infosPanel.Location = new System.Drawing.Point(-5, infosPanel.Location.Y);
            questsPanel.Location = new System.Drawing.Point(infosPanel.Width + 10, questsPanel.Location.Y);

            if (!IsPanelFullyVisible(questsPanel) || !IsPanelFullyVisible(infosPanel))
            {
                resizeToInfosPictureBox.Image = Properties.Resources.right_arrow;
                resizeToQuestsPictureBox.Image = Properties.Resources.left_arrow;
            }
            else
            {
                resizeToInfosPictureBox.Image = Properties.Resources.left_arrow;
                resizeToQuestsPictureBox.Image = Properties.Resources.right_arrow;
            }
        }

        private void resizeToQuestsBtn_Click(object sender, EventArgs e)
        {
            if (!IsPanelFullyVisible(questsPanel) || !IsPanelFullyVisible(infosPanel)) { ResizeFormToAll(); }
            else { ResizeFormToQuests(); }
        }

        private void resizeToInfosBtn_Click(object sender, EventArgs e)
        {
            if (!IsPanelFullyVisible(questsPanel) || !IsPanelFullyVisible(infosPanel)) { ResizeFormToAll(); }
            else { ResizeFormToInfos(); }
        }
        
        private bool IsPanelFullyVisible(Panel panel)
        {
            if (panel == questsPanel && questsPanel.Location.X + questsPanel.Width <= ActiveForm.Width - 10)
                return true;
            else if (panel == infosPanel && infosPanel.Location.X >= -5)
                return true;
            return false;
        }
        
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (Form1.ActiveForm == null) { return; }
            var formLoc = Form1.ActiveForm.Location;
            var cursorLoc = Cursor.Position;

            int formRightSideLoc = formLoc.X + Form1.ActiveForm.Width;
            int deltaLoc = cursorLoc.X - formLoc.X >= 0 ? cursorLoc.X - formLoc.X : formLoc.X - cursorLoc.X;
            int deltaRightSideLoc = cursorLoc.X - formRightSideLoc >= 0 ? cursorLoc.X - formRightSideLoc : formRightSideLoc - cursorLoc.X;

            int panelsDistance = questsPanel.Location.X - (infosPanel.Location.X + infosPanel.Width);

            


            if (infosPanel.Location.X > -5) //Should be 0 but it is not really the beginning of the form; -5 looks better
            {
                infosPanel.Location = new System.Drawing.Point(-5, infosPanel.Location.Y);
            }
            if (questsPanel.Location.X + questsPanel.Width < ActiveForm.Width -10)  //-10 looks better in reality than 0
            {
                questsPanel.Location = new System.Drawing.Point(ActiveForm.Width - questsPanel.Width - 10, infosPanel.Location.Y);
            }
            
            if (panelsDistance > 10) //&& IsPanelFullyVisible(questsPanel) && IsPanelFullyVisible(infosPanel))
            {
                infosPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                questsPanel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                if (deltaLoc <= 20)
                {
                    if (!IsPanelFullyVisible(infosPanel))
                    {
                        infosPanel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                        questsPanel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                    }
                }
                if (deltaRightSideLoc <= 20)
                {
                    if (!IsPanelFullyVisible(questsPanel))
                    {
                        infosPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                        questsPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    }
                }
            }
            else
            {
                if (deltaLoc <= 20)
                {
                    if (!IsPanelFullyVisible(questsPanel)) //OPTION 1
                    {
                        infosPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                        questsPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    }
                    else
                    {
                        infosPanel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                        questsPanel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                    }

                    //if (infosPanel.Location.X < 0)                                        //OPTION 2
                    //{
                    //    infosPanel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                    //    questsPanel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                    //}
                    //else
                    //{
                    //    infosPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    //    questsPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    //}
                }
                if (deltaRightSideLoc <= 20)
                {
                    if (!IsPanelFullyVisible(infosPanel))
                    {
                        infosPanel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                        questsPanel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                    }
                    else
                    {
                        infosPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                        questsPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    }

                    //if (questsPanel.Location.X + questsPanel.Width > ActiveForm.Width - 20)
                    //{
                    //    infosPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    //    questsPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                    //}
                    //else
                    //{
                    //    infosPanel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                    //    questsPanel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                    //}
                }
            }
            
            
        }
        
        #endregion

    }

    public static class Prompt
    {
        public static int ShowDialog(string text, string caption)
        {
            Form prompt = new Form();
            prompt.Width = 500;
            prompt.Height = 200;
            prompt.Text = caption;

            Font font = new Font("Monotype Corsiva", 13);
            font = new Font(font, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline);
            Label lbl_NewFileName = new Label() { Left = 10, Top = 10, Text = "File Name" };
            lbl_NewFileName.Font = font;

            NumericUpDown inputBox = new NumericUpDown() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(lbl_NewFileName);
            prompt.Controls.Add(inputBox);
            prompt.ShowDialog();
            return (int)inputBox.Value;
        }
    }

    public static class Extensions
    {
        /// <summary>
        /// Replaces an old key in a dictionary with a new key, keeping the value.
        /// </summary>
        /// <returns>Returns false if the old key does not exist in the dictionary; otherwise true.</returns>
        public static bool ChangeKey<TKey, TValue>(this IDictionary<TKey, TValue> dict,
                                           TKey oldKey, TKey newKey)
        {
            TValue value;
            if (!dict.TryGetValue(oldKey, out value))
                return false;

            dict.Remove(oldKey);
            dict[newKey] = value;  // or dict.Add(newKey, value) depending on ur comfort
            return true;
        }
    }
}

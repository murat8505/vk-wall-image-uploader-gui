﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MakiLoveBringer.Properties;
using xNet;

namespace MakiLoveBringer
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private const string ACCESS_TOKEN = "your token here";

        private List<string> pictureList;
        private int uploadCount = 0;
        private int filesToUpload = 0;
        private string _message;
        private int interval;
        private int rawInterval;
        private string group_id;
        private int group_choice = 0;
        private DateTime SendTime;
        private bool isSending;
        private int makiCount;

        private void browseBTN_Click(object sender, EventArgs e)
        {

            pictureList = new List<string>();

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                picturesFolderTB.Text = fbd.SelectedPath;
                var files = Directory.GetFiles(fbd.SelectedPath, "*.*").Where(s => s.EndsWith(".jpg") || s.EndsWith(".png") || s.EndsWith(".jpeg") || s.EndsWith(".gif"));

                foreach (string file in files)
                {
                    var length = new FileInfo(file).Length;
                    if (length > 1000)
                        pictureList.Add(file);
                }
                startBTN.Enabled = true;

                filesToUploadLBL.Text = "files to upload: " + pictureList.Count.ToString();
                statusLBL.Text = "status: waiting for start...";
            } 
        }

        private void startBTN_Click(object sender, EventArgs e)
        {
            if (startBTN.Text == "Start")
            {
                if (int.Parse(intervalTB.Text) > 0)
                {
                    if (group_choice == 0) //maki
                    {
                        makiCount = int.Parse(getMakiInfo()) + 1;
                        group_id = "114394766";
                    }

                    else if (group_choice == 1) //hentai
                    {
                        _message = Uri.EscapeDataString(messageTB.Text);
                        group_id = "113447877";
                    }

                    else if (group_choice == 2) //animebarn
                    {
                        _message = Uri.EscapeDataString(messageTB.Text);
                        group_id = "89912953";
                    }

                    else if (group_choice == 3) //loli
                    {
                        _message = Uri.EscapeDataString(messageTB.Text);
                        group_id = "108605919";
                    }

                    

                    filesToUpload = pictureList.Count;
                    filesToUploadLBL.Text = "files to upload: " + filesToUpload;
                    startBTN.Text = "Stop";
                    interval = int.Parse(intervalTB.Text) * 1000;
                    rawInterval = int.Parse(intervalTB.Text);
                    mainTimer.Interval = interval;
                    mainTimer.Start();
                    SendTime = DateTime.Now.AddMilliseconds(mainTimer.Interval);
                    statusUpdateTimer.Start();
                    statusLBL.Text = "status: waiting...";
                }
            }

            else
            {
                startBTN.Text = "Start";
                mainTimer.Stop();
                statusUpdateTimer.Stop();
            }   
        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            SendTime = DateTime.Now.AddMilliseconds(mainTimer.Interval);
            Thread t = new Thread(() => uploadShit());
            t.IsBackground = true;
            t.Start();
        }

        private void statusUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (!isSending)
            {
                TimeSpan nextUpload = SendTime - DateTime.Now;
                nextUploadLBL.Text = string.Format("time till next upload: {0}h {1}m {2}s", nextUpload.Hours, nextUpload.Minutes, nextUpload.Seconds);
            }
        }

        private void uploadShit()
        {
            if (group_choice == 0) //maki
            {
                _message = Uri.EscapeDataString("[ЕжедневнаяМаки #" + makiCount + "]" + Environment.NewLine + Environment.NewLine + "#LoveLive #NishikinoMaki");
                makiCount++;
            }

            statusLBL.Invoke(new MethodInvoker(() => statusLBL.Text = "status: uploading..."));
            isSending = true;
            string resp = string.Empty;
            string lastUploadedItem = string.Empty;
            lastUploadedItem = pictureList[0];
            pictureList.RemoveAt(0);

            resp = vk_requests.doWallPhotoUpload(group_id, ACCESS_TOKEN, _message, lastUploadedItem, true);

            if (resp != null)
            {
                if (pictureList.Count > 0)
                {
                    isSending = false;
                    var uploadTime = DateTime.Now;
                    uploadCount++;
                    filesToUpload = pictureList.Count;
                    uploadCountLBL.Invoke(new MethodInvoker(() => uploadCountLBL.Text = "upload count: " + uploadCount));
                    filesToUploadLBL.Invoke(new MethodInvoker(() => filesToUploadLBL.Text = "files to upload: " + filesToUpload));
                    statusLBL.Invoke(new MethodInvoker(() => statusLBL.Text = "status: waiting..."));
                }

                else
                {
                    isSending = false;
                    var uploadTime = DateTime.Now;
                    uploadCount++;
                    filesToUpload = 0;
                    uploadCountLBL.Invoke(new MethodInvoker(() => uploadCountLBL.Text = "upload count: " + uploadCount));
                    filesToUploadLBL.Invoke(new MethodInvoker(() => filesToUploadLBL.Text = "files to upload: " + filesToUpload));
                    statusLBL.Invoke(new MethodInvoker(() => statusLBL.Text = "status: waiting..."));

                    new SoundPlayer(Properties.Resources.Tutturuu_).Play();

                    mainTimer.Stop();
                    statusUpdateTimer.Stop();

                    startBTN.Invoke(new MethodInvoker(() => startBTN.Text = "Start"));
                    MessageBox.Show("Everything is uploaded! :3");
                }
            }

            else
            {
                statusLBL.Invoke(new MethodInvoker(() => statusLBL.Text = "status: error while uploading! probably limit"));
                isSending = false;
            }

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void stRadioButton1_CheckedChanged(object sender)
        {
            if (stRadioButton1.Checked)
            {
                messageTB.Visible = false;
                stLabel4.Visible = false;
                group_choice = 0;
            }
            else
            {
                stLabel4.Visible = true;
                messageTB.Visible = true;
            }
        }

        private string getMakiInfo()
        {
            HttpRequest r = new HttpRequest();
            string response = r.Get("https://api.vk.com/method/wall.get?owner_id=-114394766&count=1&offset=1").ToString();

            string text = response.Substring("\"text\":\"", "]");

            return text.Substring(text.IndexOf('#') + 1);
        }

        private void stRadioButton2_CheckedChanged(object sender)
        {
            if (stRadioButton2.Checked)
                group_choice = 1;
        }

        private void stRadioButton3_CheckedChanged(object sender)
        {
            if (stRadioButton3.Checked)
                group_choice = 2;
        }

        private void stRadioButton4_CheckedChanged(object sender)
        {
            if (stRadioButton4.Checked)
                group_choice = 3;
        }
    }
}

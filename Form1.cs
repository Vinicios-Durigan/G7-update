using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using ChaseLabs.CLUpdate;
using AutoIt;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.IO;

namespace G7_update
{
    public partial class Form1 : Form
    {
        Dispatcher dis = Dispatcher.CurrentDispatcher;
        string url = "https://www.dropbox.com/s/7zy1u9b3ox5xeun/Debug.zip?dl=1";
        string remote_version_url = "https://www.dropbox.com/s/2kodp2ykjynizdx/version?dl=1";
        string version_key = "application: ";
        string global_path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Gemini7");
        string update_path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gemini7", "G7", "update");
        string app_path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gemini7", "G7", "bin");
        string local_version_path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gemini7", "G7", "version");
        string loucher_exe = "PedidosG7.exe";
        string version;
        string dataBase = @"\g7New.sql";
        string uploadFile = @"\Update.sql";
        public Form1()
        {
            InitializeComponent();

        }
        private void CreateDataBase() {
            try
            {
                MySqlConnection connect = new MySqlConnection("server=localhost;user=root;port=3306;password=9637443Lol;");
                MySqlCommand command = new MySqlCommand("CREATE DATABASE IF NOT EXISTS g7;", connect);
                connect.Open();
                command.ExecuteNonQuery();
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tentativa de Criar a Database => " + ex);
            }
          
        }
        private void ImportUploadFromDb() {
            try
            {
                string pth = app_path + uploadFile;
                MySqlConnection connect = new MySqlConnection(ConfigurationManager.ConnectionStrings["client"].ConnectionString);
                MySqlCommand cmd = new MySqlCommand("", connect);
                string text = File.ReadAllText(pth);

                cmd.CommandText = text;

                connect.Open();
                
                cmd.ExecuteNonQuery();
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error --> " + ex);
            }
          
        }
        private void ImportFirstDatabase() {
            try
            {
                string constring = "server=localhost;user=root;pwd=9637443Lol;";
                string file = app_path + dataBase;
                using (MySqlConnection conn = new MySqlConnection(constring))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            mb.ImportFromFile(file);
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error --> " + ex);
            }
        }
        public bool chekLocalBase()
        {
          
            MySqlConnection connect = new MySqlConnection(ConfigurationManager.ConnectionStrings["client"].ConnectionString);
            try
            {
                connect.Open();
                connect.Close();
                return true;

            }
            catch (Exception)
            {
                connect.Close();
                return false;
            }
        }
        public void ConfigDataBase()
        {
            var exist = chekLocalBase();
            if (exist)
            {
                try
                {
                    importNewDb();
                    Process.Start(app_path + @"\" + loucher_exe);
                    Application.Exit();
                }
                catch (Exception)
                {
                    MessageBox.Show("Erro Entre em contato com o Administrador");
                }
               
            }
            else
            {
             
                scriptInstallMAriadb();
            }
        }
        public void importNewDb() {

            ImportUploadFromDb();
            MessageBox.Show("Importação Completa");
        }
        public void scriptInstallMAriadb()
        {
            Task.Run(() =>
            {
                lb_att.Text = "Instalando Base De Dados";
            });
           // CreateDataBase();
            ImportFirstDatabase();

        }
        public void Update()
        {
            Task.Run(() =>
            {
                dis.Invoke(() =>
                {
                    lb_att.Text = "Procurando Atualizações . . .";
                }, DispatcherPriority.Normal);
                var update = Updater.Init(url, update_path, app_path, loucher_exe);
                if(UpdateManager.CheckForUpdate(version_key, local_version_path, remote_version_url))
                {
                    dis.Invoke(() =>
                    {

                        lb_att.Text = "Atualização Encontrada ! !";

                    }, DispatcherPriority.Normal);
                    dis.Invoke(() =>
                    {

                        lb_att.Text = "Fazendo Download . . .";

                    }, DispatcherPriority.Normal);
                    update.Download();
                    dis.Invoke(() =>
                    {
                        lb_att.Text = "Configurando Arquivos . . .";
                    }, DispatcherPriority.Normal);
                    update.Unzip();
                    dis.Invoke(() =>
                    {
                        lb_att.Text = "Atualização Finalizada ! !";
                    }, DispatcherPriority.Normal);
                    update.CleanUp();
                    using (var client = new System.Net.WebClient())
                    {
                        client.DownloadFile(remote_version_url, local_version_path);
                    }
                    dis.Invoke(() =>
                    {
                        lb_att.Text = "Configurando Databases";
                    }, DispatcherPriority.Normal);

                    ConfigDataBase();
                    Application.Exit();

                }
                else
                    dis.Invoke(() =>
                    {
                        Process.Start(app_path + @"\" + loucher_exe);
                        Application.Exit();
                    }, DispatcherPriority.Normal);
            });
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (UpdateManager.CheckForUpdate(version_key, local_version_path, remote_version_url))
            {
               if(Directory.Exists(global_path)){
                    System.IO.Directory.Delete(global_path, true);
                }
                Update();
            }
            else
            {
                Update();
            }
        }
    }
}

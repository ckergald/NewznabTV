using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Xml;
using System.Net;
using System.Windows.Forms;

namespace newznabCleanup
{
    public partial class Form1 : Form
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button2.Enabled = false;
        }
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                button1.Enabled = false;
                button2.Enabled = true;
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server.  Contact administrator");
                        return false;

                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        return false;
                }
                MessageBox.Show("Unable to connect\r\nException - " + ex.Message.ToString());
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                button1.Enabled = true;
                button2.Enabled = false;
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }


        public List<string>[] Selectrage()
        {
            string query = "SELECT * FROM tvrage where rageID='-2'";

            //Create a list to store the result
            List<string>[] list = new List<string>[13];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();
            list[3] = new List<string>();
            list[4] = new List<string>();
            list[5] = new List<string>();
            list[6] = new List<string>();
            list[7] = new List<string>();
            list[8] = new List<string>();
            list[9] = new List<string>();
            list[10] = new List<string>();
            list[11] = new List<string>();
            list[12] = new List<string>();

            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    list[0].Add(dataReader["ID"] + "");
                    list[1].Add(dataReader["rageID"] + "");
                    list[2].Add(dataReader["tvdbID"] + "");
                    list[3].Add(dataReader["releasetitle"] + "");
                    list[4].Add(dataReader["description"] + "");
                    list[5].Add(dataReader["genre"] + "");
                    list[6].Add(dataReader["country"] + "");
                    list[7].Add(dataReader["imgdata"] + "");
                    list[8].Add(dataReader["createddate"] + "");
                    list[9].Add(dataReader["prevdate"] + "");
                    list[10].Add(dataReader["previnfo"] + "");
                    list[11].Add(dataReader["nextdate"] + "");
                    list[12].Add(dataReader["nextinfo"] + "");
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();
                label7.Text = list[0].Count.ToString() + "Items";
                label7.Refresh();
                Application.DoEvents();
                //return list to be displayed
                return list;
            }
            else
            {
                return list;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dbinfo();
            OpenConnection();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            CloseConnection();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<string>[] dblist = Selectrage();
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listBox4.Items.Clear();
            listBox5.Items.Clear();
            listBox6.Items.Clear();
            tb_ID.Text = "";
            tb_rageid.Text = "";
            tb_tvdbid.Text = "";
            for (int i = 0; i < dblist[3].Count(); i++)
            {
                listBox1.Items.Add(dblist[3][i].ToString());
                listBox2.Items.Add(dblist[0][i].ToString());
                listBox3.Items.Add(dblist[1][i].ToString());
                listBox4.Items.Add(dblist[2][i].ToString());
            }
            listBox1.Refresh();
        }
        private void dbinfo()
        {
            server = tb_db_host.Text;
            database = "newznab";
            uid = tb_db_user.Text;
            password = tb_db_pass.Text;
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            connection = new MySqlConnection(connectionString);
        }

        private void tb_db_host_TextChanged(object sender, EventArgs e)
        {
            dbinfo();
        }

        private void tb_db_user_TextChanged(object sender, EventArgs e)
        {
            dbinfo();
        }

        private void tb_db_pass_TextChanged(object sender, EventArgs e)
        {
            dbinfo();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            tb_ID.Text = listBox2.Items[listBox1.SelectedIndex].ToString();
            tb_rageid.Text = listBox3.Items[listBox1.SelectedIndex].ToString();
            tb_tvdbid.Text = listBox4.Items[listBox1.SelectedIndex].ToString();
            listBox5.Items.Clear();
            listBox5.Refresh();
            listBox6.Items.Clear();
            listBox6.Refresh();
            string rageresults = "";
            WebClient wc = new WebClient();
                            bool retry = true;
                            int counter = 0;
                            while (retry)
                            {
                                try
                                {
                                    wc.Encoding = System.Text.Encoding.UTF8;
                                    rageresults = wc.DownloadString("http://services.tvrage.com/feeds/full_search.php?show=" + Uri.EscapeUriString(listBox1.Items[listBox1.SelectedIndex].ToString()));
                                    retry = false;
                                }
                                catch
                                {
                                    if (counter < 5)
                                    {
                                        counter = counter + 1;
                                    }
                                }
                            }
                            XmlDocument rage = new XmlDocument();
                            rage.LoadXml(rageresults);
                            XmlNodeList ragenl = rage.SelectNodes("/Results/show");
                            if (ragenl.Count > 0)
                            {
                                for (int x = 0; x < ragenl.Count; x++)
                                {
                                    listBox5.Items.Add(ragenl[x]["name"].InnerText.ToString());
                                    listBox6.Items.Add(ragenl[x]["showid"].InnerText.ToString());
                                    listBox5.Refresh();
                                }
                            }
                            else
                            {
                                listBox5.Items.Add("NO match found");
                                listBox5.Refresh();
                            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void listBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            tb_rageid.Text = listBox6.Items[listBox5.SelectedIndex].ToString();
            tb_tvdbid.Refresh();
            Application.DoEvents();
            Update();
            List<string>[] dblist = Selectrage();
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listBox4.Items.Clear();
            listBox5.Items.Clear();
            listBox6.Items.Clear();
            tb_ID.Text = "";
            tb_rageid.Text = "";
            tb_tvdbid.Text = "";
            for (int i = 0; i < dblist[3].Count(); i++)
            {
                listBox1.Items.Add(dblist[3][i].ToString());
                listBox2.Items.Add(dblist[0][i].ToString());
                listBox3.Items.Add(dblist[1][i].ToString());
                listBox4.Items.Add(dblist[2][i].ToString());
            }
            listBox1.Refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Delete_from_table(tb_ID.Text);
        }
        public void Delete_from_table(string ID)
        {
            string query = "DELETE FROM tvrage WHERE ID='"+ID+"'";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
            List<string>[] dblist = Selectrage();
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listBox4.Items.Clear();
            listBox5.Items.Clear();
            listBox6.Items.Clear();
            tb_ID.Text = "";
            tb_rageid.Text = "";
            tb_tvdbid.Text = "";
            for (int i = 0; i < dblist[3].Count(); i++)
            {
                listBox1.Items.Add(dblist[3][i].ToString());
                listBox2.Items.Add(dblist[0][i].ToString());
                listBox3.Items.Add(dblist[1][i].ToString());
                listBox4.Items.Add(dblist[2][i].ToString());
            }
            listBox1.Refresh();
        }
        public void Update()
        {
            string query = "UPDATE tvrage SET rageID='" + tb_rageid.Text + "', tvdbID='" + tb_tvdbid.Text + "' WHERE ID='" + tb_ID.Text + "'";

            //Open connection
            if (this.OpenConnection() == true)
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }
    }
}

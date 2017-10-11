using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GuessTheAnimal
{
    public partial class Form1 : Form
    {

        #region Variables

        DataSet dsXML;

        bool gameStarted = false;
        int currentParameterIndex = 0;
        string currentParameter = string.Empty, filterQuery = string.Empty;

        #endregion

        #region Init & Events

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();

            #region Find & display all animals

            List<string> lstAnimals = new List<string>();
            lstAnimals.Add("Choose an animal in your head");
            foreach (DataRow drAnimal in dsXML.Tables["Animal"].Rows)
            {
                lstAnimals.Add(drAnimal["Name"].ToString());
            }
            lbOptions.DataSource = lstAnimals;
            lbOptions.Enabled = false;

            #endregion

            lblInstructions.Text = "From the list below, choose an animal in your head and click the Start button";
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (gameStarted == false)
            {
                btnNext.Text = "&Next";
                gameStarted = true;
                lbOptions.Enabled = true;
                GuessAnimalOrDisplayOptions();
            }
            else
            {
                if (filterQuery != string.Empty)
                    filterQuery += " AND ";

                filterQuery += currentParameter += " = '" + lbOptions.SelectedItem.ToString() + "'";

                GuessAnimalOrDisplayOptions();
            }
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads data from XML file into dsXML object
        /// </summary>
        private void LoadData()
        {
            string xmlPath = Path.Combine((new FileInfo(Application.ExecutablePath).DirectoryName), "Data.xml");
            dsXML = new DataSet();
            dsXML.ReadXml(xmlPath);
        }

        /// <summary>
        /// This method tries to guess the animal or displays next question to the user
        /// </summary>
        private void GuessAnimalOrDisplayOptions()
        {
            //Filter down the dataview on the basis of selected option
            DataView dvAnimals = new DataView(dsXML.Tables["Animal"]);
            dvAnimals.RowFilter = filterQuery;

            if (dvAnimals.Count == 1)
            {
                //We have narrowed down to a single animal. Display the name of the animal to the user.

                //Disable listbox & next button. Display restart game button.
                lbOptions.Enabled = false;
                btnNext.Enabled = false;
                btnRestart.Visible = true;

                //Display chosen animal
                lblTitle.ForeColor = Color.DarkGreen;
                lblTitle.Text = "You had chosen " + dvAnimals[0]["Name"] + "!";
                MessageBox.Show("You had chosen " + dvAnimals[0]["Name"] + "!", "We have guessed your animal!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //Set focus on the restart button
                btnRestart.Focus();
            }
            else
            {
                //Couldn't filter down to a single animal. Display the next parameter/question with distinct options.

                //Get the next parameter and update instructions
                currentParameter = dsXML.Tables["QuestionSequence"].Rows[currentParameterIndex]["ParameterName"].ToString();
                lblInstructions.Text = dsXML.Tables["QuestionSequence"].Rows[currentParameterIndex]["Question"].ToString();
                currentParameterIndex++;

                //Find distinct options
                List<string> lstOptions = new List<string>();
                foreach (DataRowView drvAnimal in dvAnimals)
                {
                    string option = drvAnimal[currentParameter].ToString();
                    if (lstOptions.Contains(option) == false)
                        lstOptions.Add(option);
                }

                if (lstOptions.Count == 1)
                {
                    //There's only once distinct choice for the current parameter. Call the function again to move to the next parameter.
                    GuessAnimalOrDisplayOptions();
                }
                else
                {
                    //Display the options to the user and set focus on the listbox
                    lbOptions.DataSource = lstOptions;
                    lbOptions.Focus();
                }
            }
        }

        #endregion
    }
}

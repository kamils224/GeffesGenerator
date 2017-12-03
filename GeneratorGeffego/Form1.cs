using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeneratorGeffego
{
    public partial class Form1 : Form
    {

        public delegate void GuiInfo();
        private GuiInfo GuiRefreshFunctions;
        private LFSR[] registers = { new LFSR(), new LFSR(), new LFSR() };
        private GeffeGenerator generator;
        private bool registersPrepared = false;
        private StreamCipher cipher;
        private bool keyLoaded = false;
        byte[] externalKey;

        public Form1()
        {
            generator = new GeffeGenerator(registers);
            cipher = new StreamCipher(generator);

            InitializeGenerator();
            InitializeComponent();
            GuiRefreshFunctions += RefreshGUI;
            GuiRefreshFunctions.Invoke();
            InitializeRegisterTextboxes();


        }


        private void InitializeRegisterTextboxes()
        {
            TextBox[] registerTextBoxes = { Register1TextBox, Register2TextBox, Register3TextBox };
            Label[] labels = { Register1InfoLabel, Register2InfoLabel, Register3InfoLabel };


            for (int i = 0; i < labels.Length; i++)
            {
                registerTextBoxes[i].Text = labels[i].Text;
            }

            registersPrepared = true;
        }

        private void InitializeGenerator()
        {
            Random rng = new Random();
            //BitArray array= new BitArray(new bool[] { true, true, false, false, true, false, false, true });
            for (int i = 0; i < 3; i++)
            {
                BitArray array = new BitArray(8);
                for (int j = 0; j < array.Length; j++)
                {
                    array[j] = Convert.ToBoolean(rng.Next(0, 2));
                }
                registers[i].SetRegisterValues(array);
            }

        }


        private Int32[] BoolToInt32(bool[] input)
        {
            if (input.Length % 32 != 0)
            {
                Int32[] ret = new Int32[(input.Length / 32)];
                for (int i = 0; i < input.Length - input.Length % 32; i += 32)
                {
                    int value = 0;
                    for (int j = 0; j < 32; j++)
                    {
                        if (input[i + j])
                        {
                            value += 1 << (31 - j);
                        }
                    }
                    ret[i / 32] = (Int32)value;
                }
                return ret;

            }
            else
            {
                Int32[] ret = new Int32[input.Length / 32];
                for (int i = 0; i < input.Length; i += 32)
                {
                    int value = 0;
                    for (int j = 0; j < 32; j++)
                    {
                        if (input[i + j])
                        {
                            value += 1 << (31 - j);
                        }
                    }
                    ret[i / 16] = (Int32)value;
                }
                return ret;
            }
        }

        private byte[] ToByteArray(bool[] input)
        {
            if (input.Length % 8 != 0)
            {
                byte[] ret = new byte[(input.Length / 8)];
                for (int i = 0; i < input.Length - input.Length % 8; i += 8)
                {
                    int value = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        if (input[i + j])
                        {
                            value += 1 << (7 - j);
                        }
                    }
                    ret[i / 8] = (byte)value;
                }
                return ret;

            }
            else
            {
                byte[] ret = new byte[input.Length / 8];
                for (int i = 0; i < input.Length; i += 8)
                {
                    int value = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        if (input[i + j])
                        {
                            value += 1 << (7 - j);
                        }
                    }
                    ret[i / 8] = (byte)value;
                }
                return ret;
            }
        }



        private bool BinaryStringToBool(char c)
        {
            if (c == '0')
                return false;
            if (c == '1')
                return true;
            else
                throw new ArgumentException();
        }

        private char BoolToChar(bool value)
        {
            if (value == false)
            {
                return '0';
            } else
            {
                return '1';
            }
        }

        private bool PrepareRegisters()
        {
            TextBox[] registerTextBoxes = { Register1TextBox, Register2TextBox, Register3TextBox };

            string pattern = @"^[0-1]+$";
            Regex regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);

            for (int i = 0; i < registerTextBoxes.Length; i++)
            {
                Match match = regex.Match(registerTextBoxes[i].Text);
                string text = registerTextBoxes[i].Text.Replace(" ", string.Empty);
                if (text.Length > 20)
                {
                    MessageBox.Show("Maksymalna długość rejestru to 20");
                    return false;
                }

                if (match.Success && text.Length > 1)
                {
                    BitArray tempArray = new BitArray(text.Length);

                    for (int j = 0; j < text.Length; j++)
                    {
                        tempArray[j] = BinaryStringToBool(text[j]);
                    }
                    registers[i].SetRegisterValues(tempArray);

                }
                else
                {
                    MessageBox.Show("Invalid data: " + registerTextBoxes[i].Name);
                    return false;
                }

            }
            GuiRefreshFunctions.Invoke();

            return true;
        }

        private bool CharToBool(char c)
        {
            if (c == '1')
            {
                return true;
            }
            else if (c == '0')
                return false;
            else
            {
                throw new ArgumentException("Char value must be 0 or 1");
            }
        }


        public void Generate()
        {
            if (checkBox1.Checked)
            {
                OutputConsole.Clear();
                OutputConsole2.Clear();
            }



            string pattern = @"^[1-9]+[0-9]*$";
            Regex regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);

            var match = regex.Match(NumOfBitsGeneratedTextBox.Text);
            if (match.Success)
            {
                if (registersPrepared)
                {

                    int numOfBits = Convert.ToInt32(NumOfBitsGeneratedTextBox.Text);

                    progressBar1.Minimum = 1;
                    progressBar1.Maximum = numOfBits + numOfBits / 8;
                    progressBar1.Value = 1;
                    progressBar1.Step = 1;

                    ProgressLabel.Text = "Trwa generowanie ciągu...";
                    ProgressLabel.Update();
                    char[] array = new char[numOfBits];
                    bool[] boolarray = new bool[numOfBits];

                    for (int i = 0; i < numOfBits; i++)
                    {
                        bool b = generator.GetFirstBitWithStep();
                        array[i] = BoolToChar(b);
                        boolarray[i] = b;
                        progressBar1.PerformStep();
                    }

                    ProgressLabel.Text = "Trwa zamiana na HEX...";
                    ProgressLabel.Update();
                    if (numOfBits == 1)
                    {
                        OutputConsole.AppendText(Convert.ToString(array[0]));
                        if (OutputConsole.Text.Length % 8 == 0 && OutputConsole.Text.Length > 0)
                        {
                            char[] temp = { OutputConsole.Text[OutputConsole.Text.Length - 1], OutputConsole.Text[OutputConsole.Text.Length - 2]
                            ,OutputConsole.Text[OutputConsole.Text.Length - 3],OutputConsole.Text[OutputConsole.Text.Length - 4],
                            OutputConsole.Text[OutputConsole.Text.Length - 5],OutputConsole.Text[OutputConsole.Text.Length - 6],
                            OutputConsole.Text[OutputConsole.Text.Length - 7],OutputConsole.Text[OutputConsole.Text.Length - 8]};
                            bool[] temp2 = new bool[8];
                            for (int i = 0; i < temp2.Length; i++)
                            {
                                temp2[i] = CharToBool(temp[i]);
                            }
                            byte[] item = ToByteArray(temp2);
                            OutputConsole2.AppendText(item[0].ToString("X") + " ");
                        }
                    }
                    else
                    {
                        OutputConsole.Text = new string(array);
                    }

                    byte[] byteArray = ToByteArray(boolarray);
                    foreach (var item in byteArray)
                    {
                        OutputConsole2.AppendText(item.ToString("X") + " ");
                        progressBar1.PerformStep();
                    }

                    ProgressLabel.Text = "Zakończono";

                }
                else
                {
                    MessageBox.Show("Najpierw przygotuj rejestry");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Błędna liczba bitów do wygenerowania");
                return;
            }
        }

        private void CreateRegistersButton_Click(object sender, EventArgs e)
        {
            registersPrepared = PrepareRegisters();
        }

        public void RefreshGUI()
        {
            Label[] labels = { Register1InfoLabel, Register2InfoLabel, Register3InfoLabel };
            for (int i = 0; i < 3; i++)
            {
                labels[i].ResetText();
            }
            bool[] newLine = { false, false, false };
            Label[] polynomialLabels = { PolynomialLabel1, PolynomialLabel2, PolynomialLabel3 };
            for (int i = 0; i < polynomialLabels.Length; i++)
            {
                polynomialLabels[i].Text = "1+";
                for (int j = 0; j < registers[i].XORPositions.Length; j++)
                {
                    if (j == registers[i].XORPositions.Length - 1)
                    {
                        polynomialLabels[i].Text += "x^" + (registers[i].XORPositions[j] + 1);
                        if (polynomialLabels[i].Text.Length > 45 && !newLine[i])
                        {
                            polynomialLabels[i].Text += Environment.NewLine;
                            newLine[i] = true;
                        }
                    }
                    else
                    {
                        polynomialLabels[i].Text += "x^" + (registers[i].XORPositions[j] + 1) + "+";
                        if (polynomialLabels[i].Text.Length > 45 && !newLine[i])
                        {
                            polynomialLabels[i].Text += Environment.NewLine;
                            newLine[i] = true;
                        }

                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                foreach (var item in registers[i].Register)
                {

                    labels[i].Text += Convert.ToByte(item).ToString();
                }
            }


        }

        private void Register1TextBox_TextChanged(object sender, EventArgs e)
        {
            RegisterLabel1.Text = "Rejestr1 - długość: " + Register1TextBox.Text.Length;
        }

        private void Register2TextBox_TextChanged(object sender, EventArgs e)
        {
            RegisterLabel2.Text = "Rejestr2 - długość: " + Register2TextBox.Text.Length;
        }

        private void Register3TextBox_TextChanged(object sender, EventArgs e)
        {
            RegisterLabel3.Text = "Rejestr3 - długość: " + Register3TextBox.Text.Length;
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            Generate();
            GuiRefreshFunctions.Invoke();
        }

        private void SaveToTxt()
        {
            string pattern = @"^[1-9]+[0-9]*$";
            Regex regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);

            var match = regex.Match(NumOfBitsGeneratedTextBox.Text);
            if (match.Success)
            {
                if (registersPrepared)
                {

                    int numOfBits = Convert.ToInt32(NumOfBitsGeneratedTextBox.Text);

                    progressBar1.Minimum = 1;
                    progressBar1.Maximum = numOfBits;
                    progressBar1.Value = 1;
                    progressBar1.Step = 1;

                    Stream myStream;
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                    saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 2;
                    saveFileDialog1.RestoreDirectory = true;

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        if ((myStream = saveFileDialog1.OpenFile()) != null)
                        {
                            ProgressLabel.Text = "Trwa generowanie ciągu...";
                            ProgressLabel.Update();
                            char[] array = new char[numOfBits];

                            for (int i = 0; i < numOfBits; i++)
                            {
                                array[i] = BoolToChar(generator.GetFirstBitWithStep());
                                progressBar1.PerformStep();
                            }

                            // Code to write the stream goes here.
                            byte[] buffer = new byte[numOfBits];
                            buffer = array.Select(c => (byte)c).ToArray();


                            myStream.Write(buffer, 0, numOfBits);


                            myStream.Close();
                            ProgressLabel.Text = "Zakończono";
                        }

                    }

                }
                else
                {
                    MessageBox.Show("Najpierw przygotuj rejestry");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Błędna liczba bitów do wygenerowania");
                return;
            }
        }

        private void SaveToBin()
        {
            string pattern = @"^[1-9]+[0-9]*$";
            Regex regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);

            var match = regex.Match(NumOfBitsGeneratedTextBox.Text);
            if (match.Success)
            {
                if (registersPrepared)
                {

                    int numOfBits = Convert.ToInt32(NumOfBitsGeneratedTextBox.Text);

                    progressBar1.Minimum = 1;
                    progressBar1.Maximum = numOfBits;
                    progressBar1.Value = 1;
                    progressBar1.Step = 1;

                    Stream myStream;
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                    saveFileDialog1.Filter = "All files (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 2;
                    saveFileDialog1.RestoreDirectory = true;

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        if ((myStream = saveFileDialog1.OpenFile()) != null)
                        {
                            ProgressLabel.Text = "Trwa generowanie ciągu...";
                            ProgressLabel.Update();
                            bool[] array = new bool[numOfBits];

                            for (int i = 0; i < numOfBits - numOfBits % 8; i++)
                            {
                                array[i] = generator.GetFirstBitWithStep();
                                progressBar1.PerformStep();
                            }

                            // Code to write the stream goes here.
                            byte[] buffer = ToByteArray(array);


                            myStream.Write(buffer, 0, buffer.Length);


                            myStream.Close();
                            ProgressLabel.Text = "Zakończono";
                            if (numOfBits % 8 != 0)
                            {
                                MessageBox.Show("Zapisano do pliku binarnego " + buffer.Length * 8 + " bitów. Ostatnie " + numOfBits % 8 + " bitów zignorowano " +
                                    "aby zachować wielokrotność 8.");
                            }

                        }

                    }

                }
                else
                {
                    MessageBox.Show("Najpierw przygotuj rejestry");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Błędna liczba bitów do wygenerowania");
                return;
            }
        }

        private void ChangeFeedbackFunction()
        {
            TextBox[] textboxes = { Function1Textbox, Function2Textbox, Function3Textbox };
            Match[] match = new Match[textboxes.Length];
            Regex regex = new Regex("^[1-9][0-9]*(,[1-9][0-9]*)*$");
            for (int i = 0; i < textboxes.Length; i++)
            {
                match[i] = regex.Match(textboxes[i].Text);
            }
            foreach (var item in match)
            {
                if (!item.Success)
                {
                    MessageBox.Show("Złe wartości funkcji, podaj stopnie wielomianu (od 1 do długości rejestru) po przecinku");
                    return;
                }
            }

            string[] s1 = textboxes[0].Text.Split(',');
            int[] intArray1 = new int[s1.Length];
            for (int i = 0; i < s1.Length; i++)
            {
                int temp = Convert.ToInt32(s1[i]) - 1;
                if (temp > registers[0].Length)
                {
                    MessageBox.Show("Rejestr jest za krótki dla wartości " + temp.ToString());
                    return;
                } else
                {
                    intArray1[i] = temp;
                }
            }
            registers[0].XORPositions = intArray1.ToArray();

            string[] s2 = textboxes[1].Text.Split(',');
            int[] intArray2 = new int[s1.Length];
            for (int i = 0; i < s2.Length; i++)
            {
                int temp = Convert.ToInt32(s2[i]) - 1;
                if (temp > registers[1].Length)
                {
                    MessageBox.Show("Rejestr jest za krótki!");
                    return;
                }
                else
                {
                    intArray2[i] = temp;
                }
            }
            registers[1].XORPositions = intArray2.ToArray();

            string[] s3 = textboxes[2].Text.Split(',');
            int[] intArray3 = new int[s3.Length];
            for (int i = 0; i < s3.Length; i++)
            {
                int temp = Convert.ToInt32(s3[i]) - 1;
                if (temp > registers[2].Length)
                {
                    MessageBox.Show("Rejestr jest za krótki!");
                    return;
                }
                else
                {
                    intArray3[i] = temp;
                }
            }
            registers[2].XORPositions = intArray3.ToArray();


            GuiRefreshFunctions.Invoke();


        }

        private void SaveRegistersToFile()
        {
            TextBox[] reg = { Register1TextBox, Register2TextBox, Register3TextBox };

            string[] regText = new string[registers.Length];

            for (int i = 0; i < registers.Length; i++)
            {

                foreach (bool item in registers[i].Register)
                {
                    char c = BoolToChar(item);
                    regText[i] += c;
                }
            }


            string textToSave = "Register1: " + regText[0] + Environment.NewLine + "Register2: " + regText[1] + Environment.NewLine
                + "Register3: " + regText[2] + Environment.NewLine + "Feedback function1: " + PolynomialLabel1.Text + Environment.NewLine +
                "Feedback function2: " + PolynomialLabel2.Text + Environment.NewLine + "Feedback function3: " + PolynomialLabel3.Text + Environment.NewLine;

            byte[] buffer = Encoding.ASCII.GetBytes(textToSave);

            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {

                    myStream.Write(buffer, 0, buffer.Length);
                    // Code to write the stream goes here.
                    myStream.Close();
                }
            }
        }

        private void SaveToTxtButton_Click(object sender, EventArgs e)
        {
            SaveToTxt();
            GuiRefreshFunctions.Invoke();
        }

        private void SaveToBinButton_Click(object sender, EventArgs e)
        {
            SaveToBin();
            GuiRefreshFunctions.Invoke();
        }



        private void ChangeFunctionsButton_Click(object sender, EventArgs e)
        {
            ChangeFeedbackFunction();

        }


        private void SaveRegisters_Click(object sender, EventArgs e)
        {
            SaveRegistersToFile();
        }

        private void EcryptButton_Click(object sender, EventArgs e)
        {
            {
                switch (EncryptModeComboBox.SelectedIndex)
                {
                    case 0:
                        EncryptToFileAndConsole();
                        break;
                    case 1:
                        EncryptToFile();
                        break;
                    case 2:
                        EncryptToConsole();
                        break;
                    default:
                        break;
                }
            }



        }

        private void EncryptToFile()
        {
            progressBar2.Minimum = 1;
            progressBar2.Maximum = 4;
            progressBar2.Value = 1;
            progressBar2.Step = 1;

            ProgressStatusLabel.Text = "Wczytywanie danych...";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();

            string text = CipherInputConsole.Text;
            byte[] inputArray = Encoding.UTF8.GetBytes(text);


            ProgressStatusLabel.Text = "Szyfrowanie...";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();
            byte[] result = cipher.Encrypt(inputArray);


            ProgressStatusLabel.Text = "Zapisywanie...";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();

            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    myStream.Write(result, 0, result.Length);

                    myStream.Close();
                }
            }

            ProgressStatusLabel.Text = "Zakończono";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();

            GuiRefreshFunctions.Invoke();

        }

        private void EncryptToFile(byte[] buffer)
        {
            try
            {
                ProgressStatusLabel.Text = "Szyfrowanie...";
                progressBar2.PerformStep();
                ProgressStatusLabel.Update();
                byte[] result = cipher.Encrypt(buffer);



                ProgressStatusLabel.Text = "Zapisywanie...";
                progressBar2.PerformStep();
                ProgressStatusLabel.Update();

                Stream myStream;
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        myStream.Write(result, 0, result.Length);

                        myStream.Close();
                    }
                }
            }
            catch (Exception e)
            {
                ProgressStatusLabel.Text = "Nie wybrano pliku";
                progressBar2.Value = progressBar2.Maximum;
                return;
            }

            ProgressStatusLabel.Text = "Zakończono";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();

            GuiRefreshFunctions.Invoke();

        }


        private void EncryptToConsole()
        {
            progressBar2.Minimum = 1;
            progressBar2.Maximum = 4;
            progressBar2.Value = 1;
            progressBar2.Step = 1;

            ProgressStatusLabel.Text = "Wczytywanie danych...";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();

            string text = CipherInputConsole.Text;
            byte[] inputArray = Encoding.GetEncoding(28591).GetBytes(text);

            ProgressStatusLabel.Text = "Szyfrowanie...";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();

            byte[] result = cipher.Encrypt(inputArray);
            CipherOutputConsole.Text = Encoding.GetEncoding(28591).GetString(result);

            ProgressStatusLabel.Text = "Zakończono";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();

            GuiRefreshFunctions.Invoke();
        }

        private void EncryptToConsole(byte[] buffer)
        {

            try
            {
                CipherInputConsole.Text = Encoding.GetEncoding(28591).GetString(buffer);

                ProgressStatusLabel.Text = "Szyfrowanie...";
                progressBar2.PerformStep();
                ProgressStatusLabel.Update();

                byte[] result = cipher.Encrypt(buffer);
                CipherOutputConsole.Text = Encoding.GetEncoding(28591).GetString(result);
            }
            catch (Exception e)
            {
                ProgressStatusLabel.Text = "Nie wybrano pliku";
                progressBar2.Value = progressBar2.Maximum;
                return;
            }
            ProgressStatusLabel.Text = "Zakończono";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();

            GuiRefreshFunctions.Invoke();
        }

        private void EncryptToFileAndConsole()
        {
            progressBar2.Minimum = 1;
            progressBar2.Maximum = 5;
            progressBar2.Value = 1;
            progressBar2.Step = 1;

            ProgressStatusLabel.Text = "Wczytywanie danych...";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();

            string text = CipherInputConsole.Text;
            byte[] inputArray = Encoding.GetEncoding(28591).GetBytes(text);

            ProgressStatusLabel.Text = "Szyfrowanie...";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();

            byte[] result = cipher.Encrypt(inputArray);
            CipherOutputConsole.Text = Encoding.GetEncoding(28591).GetString(result);



            ProgressStatusLabel.Text = "Zapisywanie...";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();

            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    myStream.Write(result, 0, result.Length);

                    myStream.Close();
                }
            }

            ProgressStatusLabel.Text = "Zakończono";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();

            GuiRefreshFunctions.Invoke();
        }
        private void EncryptToFileAndConsole(byte[] buffer)
        {
            try
            {
                CipherInputConsole.Text = Encoding.GetEncoding(28591).GetString(buffer);

                ProgressStatusLabel.Text = "Szyfrowanie...";
                progressBar2.PerformStep();
                ProgressStatusLabel.Update();

                byte[] result = cipher.Encrypt(buffer);


                CipherOutputConsole.Text = Encoding.GetEncoding(28591).GetString(result);

                ProgressStatusLabel.Text = "Zapisywanie...";
                progressBar2.PerformStep();
                ProgressStatusLabel.Update();

                Stream myStream;
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        myStream.Write(result, 0, result.Length);

                        myStream.Close();
                    }
                }
            }
            catch (Exception e)
            {
                ProgressStatusLabel.Text = "Nie wybrano pliku";
                progressBar2.Value = progressBar2.Maximum;
                return;
            }

            ProgressStatusLabel.Text = "Zakończono";
            progressBar2.PerformStep();
            ProgressStatusLabel.Update();


            GuiRefreshFunctions.Invoke();
        }

        private byte[] ReadFromFile()
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            byte[] buffer = new byte[myStream.Length];
                            int numOfBytes = myStream.Read(buffer, 0, buffer.Length);

                            myStream.Close();

                            return buffer;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            return null;
        }

        private void EncryptFromFileButton_Click(object sender, EventArgs e)
        {
            switch (EncryptModeComboBox.SelectedIndex)
            {
                case 0:

                    progressBar2.Minimum = 1;
                    progressBar2.Maximum = 5;
                    progressBar2.Value = 1;
                    progressBar2.Step = 1;
                    ProgressStatusLabel.Text = "Wczytywanie danych...";
                    progressBar2.PerformStep();
                    ProgressStatusLabel.Update();

                    EncryptToFileAndConsole(ReadFromFile());
                    break;
                case 1:

                    progressBar2.Minimum = 1;
                    progressBar2.Maximum = 5;
                    progressBar2.Value = 1;
                    progressBar2.Step = 1;
                    ProgressStatusLabel.Text = "Wczytywanie danych...";
                    progressBar2.PerformStep();
                    ProgressStatusLabel.Update();

                    EncryptToFile(ReadFromFile());

                    break;
                case 2:
                    progressBar2.Minimum = 1;
                    progressBar2.Maximum = 4;
                    progressBar2.Value = 1;
                    progressBar2.Step = 1;
                    ProgressStatusLabel.Text = "Wczytywanie danych...";
                    progressBar2.PerformStep();
                    ProgressStatusLabel.Update();

                    EncryptToConsole(ReadFromFile());
                    break;
                default:
                    break;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            OutputConsole.Clear();
            OutputConsole2.Clear();
        }


        //FKey label

        private void LoadBinKey()
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            externalKey = new byte[myStream.Length];
                            int n = myStream.Read(externalKey, 0, externalKey.Length);

                            myStream.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    return;
                }
            }

            KeyLoadedInfoLabel.Text = "Załadowano klucz";

        }

        private void LoadTxtKey()
        {


            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            byte[] buffer = new byte[myStream.Length];
                            int n = myStream.Read(buffer, 0, buffer.Length);

                            string s = Encoding.ASCII.GetString(buffer);
                            bool[] boolArray = new bool[s.Length];

                            for (int i = 0; i < boolArray.Length; i++)
                            {
                                boolArray[i] = BinaryStringToBool(s[i]);
                            }

                            externalKey = ToByteArray(boolArray);


                            myStream.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }

            KeyLoadedInfoLabel.Text = "Załadowano klucz";

        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(LoadBinFileRadiobtn.Checked)
            {
                LoadBinKey();
            }else
            {
                LoadTxtKey();
            }

        }
        private void FkConsoleEncryptButton_Click(object sender, EventArgs e)
        {
            if(externalKey!=null)
            {
                switch (FkSaveModeComboBox.SelectedIndex)
                {
                    case 0:
                        FkEncryptToFileAndConsole();
                        break;
                    case 1:
                        FkEncryptToFile();
                        break;
                    case 2:
                        FkEncryptToConsole();
                        break;
                    default:
                        break;
                }

            }else
            {
                MessageBox.Show("Nie wczytano klucza");
            }
        }

        private void FkEncryptToFileAndConsole()
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(FkInputConsole.Text);
            byte[] result = new byte[inputBytes.Length];

            int counter = 0;

            for (int i = 0; i < inputBytes.Length; i++)
            {
                int temp = inputBytes[i] ^ externalKey[counter];
                result[i] = (byte)temp;
                counter++;
                if (counter >= externalKey.Length)
                {
                    counter = 0;
                }
            }

            FkOutputConsole.Text = Encoding.GetEncoding(28591).GetString(result);

            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    myStream.Write(result, 0, result.Length);
                    myStream.Close();
                }
            }

        }


        private void FkEncryptToFileAndConsole(byte[] inputBytes)
        {
            
            byte[] result = new byte[inputBytes.Length];

            int counter = 0;

            for (int i = 0; i < inputBytes.Length; i++)
            {
                int temp = inputBytes[i] ^ externalKey[counter];
                result[i] = (byte)temp;
                counter++;
                if (counter >= externalKey.Length)
                {
                    counter = 0;
                }
            }

            FkOutputConsole.Text = Encoding.GetEncoding(28591).GetString(result);

            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    myStream.Write(result, 0, result.Length);
                    myStream.Close();
                }
            }

        }

        private void FkEncryptToFile()
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(FkInputConsole.Text);
            byte[] result = new byte[inputBytes.Length];

            int counter = 0;

            for (int i = 0; i < inputBytes.Length; i++)
            {
                int temp = inputBytes[i] ^ externalKey[counter];
                result[i] = (byte)temp;
                counter++;
                if (counter >= externalKey.Length)
                {
                    counter = 0;
                }
            }

            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    myStream.Write(result, 0, result.Length);
                    myStream.Close();
                }
            }
        }

        private void FkEncryptToFile(byte[] inputBytes)
        {
            byte[] result = new byte[inputBytes.Length];

            int counter = 0;

            for (int i = 0; i < inputBytes.Length; i++)
            {
                int temp = inputBytes[i] ^ externalKey[counter];
                result[i] = (byte)temp;
                counter++;
                if (counter >= externalKey.Length)
                {
                    counter = 0;
                }
            }

            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    myStream.Write(result, 0, result.Length);
                    myStream.Close();
                }
            }
        }

        private void FkEncryptToConsole()
        {
            byte[] inputBytes = Encoding.GetEncoding(28591).GetBytes(FkInputConsole.Text);
            byte[] result = new byte[inputBytes.Length];

            int counter = 0;

            for (int i = 0; i < inputBytes.Length; i++)
            {
                int temp = inputBytes[i] ^ externalKey[counter];
                result[i] = (byte)temp;
                counter++;
                if (counter >= externalKey.Length)
                {
                    counter = 0;
                }
            }

            FkOutputConsole.Text = Encoding.GetEncoding(28591).GetString(result);
        }

        private void FkEncryptToConsole(byte[] inputBytes)
        {
            byte[] result = new byte[inputBytes.Length];

            int counter = 0;

            for (int i = 0; i < inputBytes.Length; i++)
            {
                int temp = inputBytes[i] ^ externalKey[counter];
                result[i] = (byte)temp;
                counter++;
                if (counter >= externalKey.Length)
                {
                    counter = 0;
                }
            }

            FkOutputConsole.Text = Encoding.GetEncoding(28591).GetString(result);
        }

        private void FkFileEncryptButton_Click(object sender, EventArgs e)
        {
            switch(FkSaveModeComboBox.SelectedIndex)
            {
                case 0:
                    FkEncryptToFileAndConsole(ReadFromFile());
                    break;
                case 1:
                    FkEncryptToFile(ReadFromFile());
                    break;
                case 2:
                    FkEncryptToConsole(ReadFromFile());
                    break;
            }
        }

        private void LoadSeriesButton_Click(object sender, EventArgs e)
        {
            try
            {
                var input = ReadFromFile();
                if (input.Length != 2500)
                {
                    input = input.Take(2500).ToArray();
                }
                FipsTests test = new FipsTests();
                var single = test.SingleBitTest(input);
                var series = test.SeriesTest(input);
                var longSeries = test.LongSeriesTests(input);
                var poker = test.PokerTest(input);

                string zeroes = "";
                string ones = "";
                int counter = 1;
                foreach (var item in series.SeriesZeroArray)
                {
                    zeroes += counter + ": " + item + ", ";
                    counter++;
                }
                counter = 1;
                foreach (var item in series.SeriesOneArray)
                {
                    ones += counter + ": " + item + ", ";
                    counter++;
                }

                string pokerString = "";

                counter = 0;

                foreach (var item in poker.ValueArray)
                {
                    pokerString += counter+": " +item + ", ";
                    counter++;
                }
                counter = 1;
                string s = "";
                foreach (var item in input)
                {
                    s += Convert.ToString(item, 2).PadLeft(8,'0');
                }
                TestInputTextBox.Text = s;

                TestsResultTextBox.Text = "Test pojedynczych bitów:\nLiczba jedynek: " + single.NumberOfOneBits + Environment.NewLine
                    + "Wynik testu: " + BoolToPolishWord(single.TestPassed) + Environment.NewLine + Environment.NewLine +
                    "Test serii:\nLiczba zer: "
                    + zeroes + Environment.NewLine + "Liczba jedynek: " + ones + Environment.NewLine
                    + "Wynik testu: " + BoolToPolishWord(series.TestPassed) + Environment.NewLine + Environment.NewLine +
                    "Test długiej serii: " + Environment.NewLine +
                    "Najdłuższy ciąg: " + longSeries.LongestSeries + Environment.NewLine
                    + "Wynik testu: " + BoolToPolishWord(longSeries.TestPassed) + Environment.NewLine + Environment.NewLine +
                    "Test pokerowy: " + pokerString + "Wartość: " + poker.Result + Environment.NewLine
                    + "Wynik testu: " + BoolToPolishWord(poker.TestPassed);


            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd: " + ex.Message);
                return;
            }
        }

        private string BoolToPolishWord(bool b)
        {
            if (b)
            {
                return "Pozytywny";
            }
            else
                return "Negatywny";
        }

        private void LoadSeriesTxtButton_Click(object sender, EventArgs e)
        {
            try
            {
                var input = ReadFromFile();
                if (input.Length != 20000)
                {
                    input = input.Take(20000).ToArray();
                }

                string s = Encoding.ASCII.GetString(input);
                FipsTests test = new FipsTests();
                var single = test.SingleBitTest(s);
                var series = test.SeriesTest(s);
                var longSeries = test.LongSeriesTests(s);
                var poker = test.PokerTest(s);

                string zeroes = "";
                string ones = "";
                int counter = 1;
                foreach (var item in series.SeriesZeroArray)
                {
                    zeroes += item + ", ";
                    counter++;
                }
                counter = 1;
                foreach (var item in series.SeriesOneArray)
                {
                    ones += item + ", ";
                    counter++;
                }
                counter = 0;
                string pokerString = "";
                foreach (var item in poker.ValueArray)
                {
                    pokerString += counter + ": "+item + ", ";
                    counter++;
                }

                TestInputTextBox.Text = s;

                TestsResultTextBox.Text = "Test pojedynczych bitów:\nLiczba jedynek: " + single.NumberOfOneBits + Environment.NewLine
                                    + "Wynik testu: " + BoolToPolishWord(single.TestPassed) + Environment.NewLine + Environment.NewLine +
                                    "Test serii:\nLiczba zer: "
                                    + zeroes + Environment.NewLine + "Liczba jedynek: " + ones + Environment.NewLine
                                    + "Wynik testu: " + BoolToPolishWord(series.TestPassed) + Environment.NewLine + Environment.NewLine +
                                    "Test długiej serii: " + Environment.NewLine +
                                    "Najdłuższy ciąg: " + longSeries.LongestSeries + Environment.NewLine
                                    + "Wynik testu: " + BoolToPolishWord(longSeries.TestPassed) + Environment.NewLine + Environment.NewLine +
                                    "Test pokerowy: " + pokerString + "Wartość: " + poker.Result + Environment.NewLine
                                    + "Wynik testu: " + BoolToPolishWord(poker.TestPassed);


            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił błąd: " + ex.Message);
                return;
            }
        }

        //unused methods
        private void RegisterLabel1_Click(object sender, EventArgs e)
        {
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }


        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }



        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ReadKeyFromFile_Click(object sender, EventArgs e)
        {

        }

        private void FkSaveModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


    }
}

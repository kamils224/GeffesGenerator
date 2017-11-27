using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorGeffego
{
    class LFSR
    {
        
        public BitArray Register { get; private set; }
        public int Length { get; private set; }
        public int[] XORPositions { get; set; }


        public LFSR()
        {
            Register = new BitArray(8);

            this.Length = 8;

            Random rng = new Random();


            for (int i = 0; i < Length; i++)
            {
                Register[i] = rng.Next(0, 2) > 0;

            }

            FeedbackFunction();

        }

        public LFSR(int length)
        {
            Register = new BitArray(8);

            this.Length = length;
            Register = new BitArray(length);

            Random rng = new Random();


            for (int i = 0; i < Length; i++)
            {
                Register[i] = rng.Next(0, 2) > 0;

            }

            FeedbackFunction();

        }

        private void FeedbackFunction()
        {
            switch (Length)
            {
                case 2:
                    XORPositions = new int[] { 0, Length - 1 };
                    break;
                case 3:
                    XORPositions = new int[] { 0, Length - 1 };
                    break;
                case 4:
                    XORPositions = new int[] { 0, Length - 1 };
                    break;
                case 5:
                    XORPositions = new int[] { 0, 2 };
                    break;
                case 6:
                    XORPositions = new int[] { 0, Length - 1 };
                    break;
                case 7:
                    XORPositions = new int[] { 0, Length - 1 };
                    break;
                case 8:
                    XORPositions = new int[] { 0, 1, 6, 7 };
                    break;
                case 9:
                    XORPositions = new int[] { 0, 4 };
                    break;
                case 10:
                    XORPositions = new int[] { 0, 3 };
                    break;
                case 11:
                    XORPositions = new int[] { 0, 8, 10 };
                    break;
                case 12:
                    XORPositions = new int[] { 0, 3, 9, 10, 11 };
                    break;
                case 13:
                    XORPositions = new int[] { 0, 2, 4, 5, 6, 11 };
                    break;
                case 14:
                    XORPositions = new int[] { 0, 3, 4, 5 };
                    break;
                case 15:
                    XORPositions = new int[] { 0, 1, 13, 14 };
                    break;
                case 16:
                    XORPositions = new int[] { 0, 3, 6 };
                    break;
                case 17:
                    XORPositions = new int[] { 0, 6, 7, 8 };
                    break;
                case 18:
                    XORPositions = new int[] { 0, 1, 2, 8 };
                    break;
                case 19:
                    XORPositions = new int[] { 0, 1, 5, 18 };
                    break;
                case 20:
                    XORPositions = new int[] { 0, 1, 2, 8, 17, 19 };
                    break;

                default:
                    break;
            }
        }

        public void SetRegisterValues(BitArray array)
        {
            Register = new BitArray(array.Length);
            Length = array.Length;
            for (int i = 0; i < Length; i++)
            {
                Register[i] = array[i];
            }

            FeedbackFunction();
        }

        public void NextStep()
        {
            //funkcja zwrotna

            bool temp = true;


            for (int i = 0; i < XORPositions.Length; i++)
            {
                temp ^= Register[XORPositions[i]];
            }


            for (int i = 1; i < Register.Count; i++)
            {
                Register[i - 1] = Register[i];
            }


            Register[Register.Count - 1] = temp;


        }
        public bool GetOutputBit()
        {
            return Register[Register.Length-1];
        }

    }

}

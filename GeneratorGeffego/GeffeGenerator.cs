using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorGeffego
{
    class GeffeGenerator
    {
        private LFSR[] lfsr = new LFSR[3];

        public GeffeGenerator()
        {
            for (int i = 0; i < 3; i++)
            {
                lfsr[i] = new LFSR();
            }
        }

        public GeffeGenerator(LFSR[] registers)
        {
            if (registers.Length != 3)
            {
                throw new ArgumentException();
            }
            else
            {
                lfsr = registers;
            }
        }

       
        public void ChangeRegisters(LFSR[] registers)
        {
            if(registers.Length!=3)
            {
                throw new ArgumentException();
            }else
            {
                lfsr = registers;
            }
        }

        public void ChangeRegisters(int registerNumber, LFSR register)
        {
            if(registerNumber<1||registerNumber>3)
            {
                throw new ArgumentException("Register number must be 1-3");
            }else
            {
                lfsr[registerNumber - 1] = register;
            }
        }

        public bool GetFirstBit()
        {
            bool[] a = { lfsr[0].GetOutputBit(), lfsr[1].GetOutputBit(), lfsr[2].GetOutputBit() };
            bool result = (a[2] & a[0]) | ((!a[0]) & a[1]);

            return result;
        }

        public void NextStep()
        {
            for (int j = 0; j < lfsr.Length; j++)
            {
                lfsr[j].NextStep();
            }
        }

        public bool GetFirstBitWithStep()
        {
            bool temp = GetFirstBit();
            NextStep();
            return temp;
        }
    }
}

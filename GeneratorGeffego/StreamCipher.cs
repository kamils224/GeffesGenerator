using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorGeffego
{
    class StreamCipher
    {
        private GeffeGenerator generator;

        public StreamCipher(GeffeGenerator gen)
        {
            generator = gen;

        }


        public void SetRegisters(int numOfRegister,BitArray array)
        {
            if(array.Length<2||array.Length>20)
            {
                throw new ArgumentException("Array length 2-20");
            }else
            {
                LFSR lfsr = new LFSR(array.Length);
                lfsr.SetRegisterValues(array);
                generator.ChangeRegisters(numOfRegister, lfsr);

            }
        }

        private byte EncryptOneByte(ref byte b)
        {
            bool[] boolArray = new bool[8];
            for (int i = 0; i < 8; i++)
            {
                boolArray[i]=generator.GetFirstBitWithStep();
            }
            byte[] key = BooleanArrayToByte(boolArray);
            int result = b ^ key[0];

            return (byte)result;
        }

        public byte[] Encrypt(byte[] inputArray)
        {
            byte[] result = new byte[inputArray.Length];
            for (int i = 0; i < inputArray.Length; i++)
            {
                result[i] = EncryptOneByte(ref inputArray[i]);
            }
            return result;
        }
        


        private byte[] BooleanArrayToByte(bool[] input)
        {
            if(input.Length%8!=0)
            {
                throw new ArgumentException(input.Length.ToString());
            }else
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



        public byte[] EncryptWithExternalKey(byte[] input,byte[] key)
        {
            if(input.Length>key.Length)
            {
                byte[] newKey = new byte[input.Length];

                int j = 0;
                for (int i = 0; i < newKey.Length; i++)
                {
                    newKey[i] = key[j];
                    j++;
                    if (j > key.Length)
                        j = 0;
                }

                key = newKey;
            }

            byte[] resultArray = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                int r = input[i] ^ key[i];
                resultArray[i] = (byte)r;
            }
            return resultArray;

        }



    }
}

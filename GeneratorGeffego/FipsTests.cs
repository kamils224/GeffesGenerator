using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorGeffego
{
    public class SingleBitTestResult
    {
        public bool TestPassed { get; set; }
        public int NumberOfOneBits { get; set; }
    }

    public class SeriesTestResult
    {
        public bool TestPassed { get; set; }
        public int[] SeriesOneArray { get; set; }
        public int[] SeriesZeroArray { get; set; }
    }

    public class LongSeriesTestResult
    {
        public bool TestPassed { get; set; }
        public int LongestSeries { get; set; }
    }

    public class PokerTestResult
    {
        public bool TestPassed { get; set; }
        public int[] ValueArray { get; set; }
        public double Result { get; set; }
    }

    class FipsTests
    {
        private const int bitsLength = 20000;


        public SingleBitTestResult SingleBitTest(byte[] input)
        {
            if(input.Length!=bitsLength/8)
            {
                throw new ArgumentException("Zła długość tablicy");
            }

            int Result = 0;

            for (int i = 0; i < input.Length; i++)
            {
                int count = Convert.ToString(input[i], 2).ToCharArray().Count(c => c == '1');
                Result += count;
            }

            SingleBitTestResult res = new SingleBitTestResult();
            res.NumberOfOneBits = Result;
            if (Result > 9725 && Result < 10275)
            {
                res.TestPassed = true;
            }else
            {
                res.TestPassed = false;
            }
            return res;
        }

        public SingleBitTestResult SingleBitTest(string input)
        {
            foreach (var item in input)
            {
                if(item!='1' && item!='0')
                {
                    throw new ArgumentException("Znaleziono zły znak: "+item);
                }
            }

            int Result = 0;

            Result = input.Count(c => c == '1');

            SingleBitTestResult res = new SingleBitTestResult();
            res.NumberOfOneBits = Result;
            if (Result > 9725 && Result < 10275)
            {
                res.TestPassed = true;
            }
            else
            {
                res.TestPassed = false;
            }
            return res;
        }


        public SeriesTestResult SeriesTest(byte[] inputBytes)
        {
            if (inputBytes.Length != bitsLength / 8)
            {
                throw new ArgumentException("Length of array must be 20000/8");
            }

            string input = String.Join("", inputBytes.Select(x => Convert.ToString(x, 2).PadLeft(8, '0')));

            int[] seriesOneArray = new int[6] { 0, 0, 0, 0, 0, 0 };
            int[] seriesZeroArray = new int[6] { 0, 0, 0, 0, 0, 0 };

            int series = 1;

            char temp = input[0];
            bool isOne = temp == '1';

            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] == temp)
                {
                    series++;
                    temp = input[i];
                }
                else
                {
                    if (series >= 6)
                    {
                        if (isOne)
                            seriesOneArray[5]++;
                        else
                            seriesZeroArray[5]++;
                    }
                    else
                    {
                        if (isOne)
                            seriesOneArray[series - 1]++;
                        else
                            seriesZeroArray[series - 1]++;
                    }

                    temp = input[i];
                    series = 1;
                    isOne = !isOne;

                }

                if (i == input.Length - 1)
                {
                    if (series >= 6)
                    {
                        if (isOne)
                            seriesOneArray[5]++;
                        else
                            seriesZeroArray[5]++;
                    }
                    else
                    {
                        if (isOne)
                            seriesOneArray[series - 1]++;
                        else
                            seriesZeroArray[series - 1]++;
                    }
                }
            }

            SeriesTestResult result = new SeriesTestResult();
            result.SeriesOneArray = seriesOneArray;
            result.SeriesZeroArray = seriesZeroArray;

            if (seriesOneArray[0] >= 2315 && seriesOneArray[0] <= 2685 && seriesOneArray[1] >= 1114 && seriesOneArray[1] <= 1386 && seriesOneArray[2] >= 527 && seriesOneArray[2] <= 723 &&
                seriesOneArray[3] >= 240 && seriesOneArray[3] <= 384 && seriesOneArray[4] >= 103 && seriesOneArray[4] <= 209 && seriesOneArray[5] >= 103 && seriesOneArray[5] <= 209 &&
                seriesZeroArray[0] >= 2315 && seriesZeroArray[0] <= 2685 && seriesZeroArray[1] >= 1114 && seriesZeroArray[1] <= 1386 && seriesZeroArray[2] >= 527 && seriesZeroArray[2] <= 723 &&
                seriesZeroArray[3] >= 240 && seriesZeroArray[3] <= 384 && seriesZeroArray[4] >= 103 && seriesZeroArray[4] <= 209 && seriesZeroArray[5] >= 103 && seriesZeroArray[5] <= 209)
            {
                result.TestPassed = true;
            }
            else
            {
                result.TestPassed = false;
            }

            return result;

        }


        public SeriesTestResult SeriesTest(string input)
        {
            if (input.Length != bitsLength)
            {
                throw new ArgumentException("Length of array must be 20000/8");
            }

            foreach (var item in input)
            {
                if (item != '1' && item != '0')
                {
                    throw new ArgumentException("Znaleziono zły znak: " + item);
                }
            }

            int[] seriesOneArray = new int[6] { 0, 0, 0, 0, 0, 0 };
            int[] seriesZeroArray = new int[6] { 0, 0, 0, 0, 0, 0 };

            int series = 1;
            
            char temp = input[0];


            bool isOne=temp=='1';



            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] == temp)
                {
                    series++;
                    temp = input[i];
                }
                else
                {
                    if (series >= 6)
                    {
                        if (isOne)
                            seriesOneArray[5]++;
                        else
                            seriesZeroArray[5]++;
                    }
                    else
                    {
                        if (isOne)
                            seriesOneArray[series - 1]++;
                        else
                            seriesZeroArray[series - 1]++;
                    }

                    temp = input[i];
                    series = 1;
                    isOne = !isOne;

                }

                if (i == input.Length - 1)
                {
                    if (series >= 6)
                    {
                        if (isOne)
                            seriesOneArray[5]++;
                        else
                            seriesZeroArray[5]++;
                    }
                    else
                    {
                        if (isOne)
                            seriesOneArray[series - 1]++;
                        else
                            seriesZeroArray[series - 1]++;
                    }
                }
            }

            SeriesTestResult result = new SeriesTestResult();
            result.SeriesOneArray = seriesOneArray;
            result.SeriesZeroArray = seriesZeroArray;

            if (seriesOneArray[0] >= 2315 && seriesOneArray[0] <= 2685 && seriesOneArray[1] >= 1114 && seriesOneArray[1] <= 1386 && seriesOneArray[2] >= 527 && seriesOneArray[2] <= 723 &&
                seriesOneArray[3] >= 240 && seriesOneArray[3] <= 384 && seriesOneArray[4] >= 103 && seriesOneArray[4] <= 209 && seriesOneArray[5] >= 103 && seriesOneArray[5] <= 209&&
                seriesZeroArray[0] >= 2315 && seriesZeroArray[0] <= 2685 && seriesZeroArray[1] >= 1114 && seriesZeroArray[1] <= 1386 && seriesZeroArray[2] >= 527 && seriesZeroArray[2] <= 723 &&
                seriesZeroArray[3] >= 240 && seriesZeroArray[3] <= 384 && seriesZeroArray[4] >= 103 && seriesZeroArray[4] <= 209 && seriesZeroArray[5] >= 103 && seriesZeroArray[5] <= 209)
            {
                result.TestPassed = true;
            }
            else
            {
                result.TestPassed = false;
            }

            return result;
        }


        public LongSeriesTestResult LongSeriesTests(byte[] input)
        {
            if (input.Length != bitsLength / 8)
            {
                throw new ArgumentException("Length of array must be 20000/8");
            }

            string s = String.Join("", input.Select(x => Convert.ToString(x, 2).PadLeft(8, '0')));

            char temp = s[0];
            int series = 1;
            int maxSeries = 1;

            for (int i = 1; i < s.Length; i++)
            {
                if(s[i]==temp)
                {
                    series++;
                    temp = s[i];
                }else
                {
                    if(series>maxSeries)
                        maxSeries = series;
                    series = 1;
                    temp = s[i];
                }
            }

            LongSeriesTestResult result = new LongSeriesTestResult();
            result.LongestSeries = maxSeries;
            if (maxSeries >= 26)
                result.TestPassed = false;
            else
                result.TestPassed = true;

            return result;
        }


        public LongSeriesTestResult LongSeriesTests(string s)
        {
            if (s.Length != bitsLength)
            {
                throw new ArgumentException("Length of array must be 20000/8");
            }

            foreach (var item in s)
            {
                if (item != '1' && item != '0')
                {
                    throw new ArgumentException("Znaleziono zły znak: " + item);
                }
            }


            char temp = s[0];
            int series = 1;
            int maxSeries = 1;

            for (int i = 1; i < s.Length; i++)
            {
                if (s[i] == temp)
                {
                    series++;
                    temp = s[i];
                }
                else
                {
                    if (series > maxSeries)
                        maxSeries = series;
                    series = 1;
                    temp = s[i];
                }
            }

            LongSeriesTestResult result = new LongSeriesTestResult();
            result.LongestSeries = maxSeries;
            if (maxSeries >= 26)
                result.TestPassed = false;
            else
                result.TestPassed = true;

            return result;
        }

        public PokerTestResult PokerTest(byte[] input)
        {
            if (input.Length != bitsLength / 8)
            {
                throw new ArgumentException("Length of array must be 20000/8");
            }


            string s = String.Join("", input.Select(x => Convert.ToString(x, 2).PadLeft(8, '0')));
            Console.WriteLine(s);
            int[] valueArray = new int[16];

            for (int i = 0; i < s.Length; i += 4)
            {
                string tempString = s.Substring(i, 4);
                byte value = Convert.ToByte(tempString, 2);

                valueArray[value]++;

            }

            int sum = 0;
            for (int i = 0; i < valueArray.Length; i++)
            {
                sum += (valueArray[i])* (valueArray[i]);
            }
            double X = (16.0 / 5000.0) * sum-5000.0;


            PokerTestResult result = new PokerTestResult();
            result.ValueArray = valueArray;
            result.Result = X;
            if(X>2.16 && X<46.17)
            {
                result.TestPassed = true;
            }else
            {
                result.TestPassed = false;
            }

            return result;

        }

        public PokerTestResult PokerTest(string input)
        {
            if (input.Length != bitsLength)
            {
                throw new ArgumentException("Length of string must be 20000");
            }

            foreach (var item in input)
            {
                if (item != '1' && item != '0')
                {
                    throw new ArgumentException("Znaleziono zły znak: " + item);
                }
            }

            int[] valueArray = new int[16];

            for (int i = 0; i < input.Length; i += 4)
            {
                string tempString = input.Substring(i, 4);
                int value = Convert.ToInt32(tempString, 2);

                valueArray[value]++;

            }

            int sum = 0;
            for (int i = 0; i < valueArray.Length; i++)
            {
                sum += (valueArray[i] * valueArray[i]);
            }
            double X = (16.0 / 5000.0) * sum-5000.0;

            PokerTestResult result = new PokerTestResult();
            result.ValueArray = valueArray;
            result.Result = X;
            if (X > 2.16 && X < 46.17)
            {
                result.TestPassed = true;
            }
            else
            {
                result.TestPassed = false;
            }

            return result;
        }
    }
}

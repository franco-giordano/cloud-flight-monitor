using Mighty.HID;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudFlightMonitor.model
{
    class CloudFlightHeadset
    {
        private HIDDev dev;

        public CloudFlightHeadset()
        {
            List<HIDInfo> allDevs = HIDBrowse.Browse();
            allDevs.ForEach(x => Console.WriteLine("{0} - {1} - {2} - {3} - {4} - {5}", x.Manufacturer, x.Path, x.Product, x.SerialNumber, x.Pid, x.Vid));
            Console.WriteLine("CONSTRUCTOR: Scanning matching devices...");
            List<HIDInfo> devs = allDevs.FindAll(x => x.Pid == 5923 && x.Vid == 2385);
            List<HIDInfo> chargingDevs = allDevs.FindAll(x => x.Pid == 5925 && x.Vid == 2385);

            if (chargingDevs.Count != 0)
            {
                Console.WriteLine("CONSTRUCTOR: Cloud Flight Device Charging!");
                MakeConnection(chargingDevs, 1);
                return;
            }

            if (devs.Count == 0)
            {
                Console.WriteLine("CONSTRUCTOR: Cloud Flight Device not found!");
                throw new Exception("Device not found");
            }

            MakeConnection(devs, 2);
        }

        private void MakeConnection(List<HIDInfo> devs, int index)
        {
            Console.WriteLine("CONSTRUCTOR: Found devices. Count: {0}", devs.Count);

            dev = new HIDDev();
            bool res = dev.Open(devs.ElementAt(index));
            Console.WriteLine("CONSTRUCTOR: Open connection result is {0}", res);

            if (!res)
            {
                throw new Exception("Device found, but couldn't connect");
            }
        }

        public int ReadBattery()
        {
            // get report
            byte[] reportIn = GetReport();

            Console.WriteLine(string.Join("\t", reportIn));

            // TODO: check report length

            Int32 chargeState = reportIn[3];
            Int32 magicValue = reportIn[4] != 0 ? reportIn[4] : chargeState;

            Int32 percentage = CalculatePercentage(chargeState, magicValue);

            Console.WriteLine("Charge: " + chargeState + " - MV: " + magicValue + " - Percentage: " + percentage);

            return percentage;
        }

        private byte[] GetReport()
        {
            byte[] report = new byte[20];
            report[0] = 0x21;
            report[1] = 0xff;
            report[2] = 0x05;

            // Send request
            dev.Write(report);

            // Prepare buffer for answer
            byte[] reportIn = new byte[20];

            // Read answer
            dev.Read(reportIn);

            return reportIn;
        }

        private int CalculatePercentage(int chargeState, int magicValue)
        {

            if (chargeState == 0x10)
            {
                if (magicValue <= 11)
                {
                    return 200; // full?
                }
                return 199; // charging
            }


            if (chargeState == 0xf)
            {
                if (magicValue >= 130)
                {
                    return 100;
                }

                if (magicValue < 130 && magicValue >= 120)
                {
                    return 95;
                }

                if (magicValue < 120 && magicValue >= 100)
                {
                    return 90;
                }

                if (magicValue < 100 && magicValue >= 70)
                {
                    return 85;
                }

                if (magicValue < 70 && magicValue >= 50)
                {
                    return 80;
                }

                if (magicValue < 50 && magicValue >= 20)
                {
                    return 75;
                }

                if (magicValue < 20 && magicValue > 0)
                {
                    return 70;
                }
            }
            if (chargeState == 0xe)
            {
                if (magicValue < 250 && magicValue > 240)
                {
                    return 65;
                }

                if (magicValue < 240 && magicValue >= 220)
                {
                    return 60;
                }

                if (magicValue < 220 && magicValue >= 208)
                {
                    return 55;
                }

                if (magicValue < 208 && magicValue >= 200)
                {
                    return 50;
                }

                if (magicValue < 200 && magicValue >= 190)
                {
                    return 45;
                }

                if (magicValue < 190 && magicValue >= 180)
                {
                    return 40;
                }

                if (magicValue < 179 && magicValue >= 169)
                {
                    return 35;
                }

                if (magicValue < 169 && magicValue >= 159)
                {
                    return 30;
                }

                if (magicValue < 159 && magicValue >= 148)
                {
                    return 25;
                }

                if (magicValue < 148 && magicValue >= 119)
                {
                    return 20;
                }

                if (magicValue < 119 && magicValue >= 90)
                {
                    return 15;
                }

                if (magicValue < 90)
                {
                    return 10;
                }

                // TODO:
                // there are values of magicValue > 250 with a charge level 
                // of round about 65-70... 
                return 66;
            }
            return 255; //error
        }
    }
}

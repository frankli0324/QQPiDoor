using System;
using System.IO;
using System.Threading.Tasks;
using cqhttp.Cyan;
using cqhttp.Cyan.Enums;

namespace qdcontroller
{
    public class DoorKeeper
    {
        static object door_lock = new object ();
        static Random random = new Random (9123);
        static void OperateGPIO (string dest, string value) {
            File.WriteAllText ("/sys/class/gpio/" + dest, value);
        }
        public static async Task OpenDoor () {
            await Task.Run (() => {
                lock (door_lock) {
                    if (!Directory.Exists ("/sys/class/gpio/gpio26")) {
                        Logger.Log (Verbosity.DEBUG, "export gpio 26");
                        OperateGPIO ("export", "26");
                    }
                    Console.WriteLine (random.Next (2) == 0 ? "Hacker time!QwQ" : "Open, Sesame!");

                    OperateGPIO ("gpio26/direction", "out");

                    OperateGPIO ("gpio26/value", "0");
                    System.Threading.Thread.Sleep (500);
                    OperateGPIO ("gpio26/value", "1");
                    System.Threading.Thread.Sleep (500);
                    OperateGPIO ("gpio26/value", "0");
                    System.Threading.Thread.Sleep (500);
                    OperateGPIO ("gpio26/value", "1");

                    if (Directory.Exists ("/sys/class/gpio/gpio26")) {
                        Logger.Log (Verbosity.DEBUG, "unexport gpio 26");
                        OperateGPIO ("unexport", "26");
                    }
                }
            });
        }
    }
}
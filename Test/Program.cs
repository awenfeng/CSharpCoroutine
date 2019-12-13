using System;
using System.Collections;
using System.Threading;
using CSharpCoroutine;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Coroutine sample begin");

            var coroutineMain = CoroutineManager.Instance.StartCoroutine(CoroutineFuncMain());
            while (true)
            {
                CoroutineManager.Instance.Update();

                if (coroutineMain.IsDone())
                    break;

                Thread.Sleep(200);
            }

            Console.WriteLine($"Coroutine sample end");

            Console.ReadKey();
        }

        static IEnumerator CoroutineFuncMain()
        {
            Coroutine[] coroutines = new Coroutine[]
            {
                CoroutineManager.Instance.StartCoroutine(Coroutine1()),
                CoroutineManager.Instance.StartCoroutine(Coroutine2()),
                CoroutineManager.Instance.StartCoroutine(Coroutine3()),
            };

            yield return new CoroutineGroup(coroutines);
        }

        static IEnumerator Coroutine1()
        {
            Console.WriteLine("Coroutine1 start...");
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Coroutine1 {i}");
                yield return new WaitForSeconds(1.0f);
            }

            Console.WriteLine("Coroutine1 end");
            yield break;
        }

        static AutoResetEvent testEvent = new AutoResetEvent(false);
        static IEnumerator Coroutine2()
        {
            Console.WriteLine("Coroutine2 start...");
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForFrame();
                Console.WriteLine($"Coroutine2 {i}");
            }

            testEvent.Set();

            Console.WriteLine("Coroutine2 end");
            yield break;
        }

        static IEnumerator Coroutine3()
        {
            Console.WriteLine("Coroutine3 start, wait for event...");

            yield return new WaitForEvent(testEvent, 30.0);
            
            Console.WriteLine("Coroutine3 end");
            yield break;
        }
    }
}

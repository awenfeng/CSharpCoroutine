using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace CSharpCoroutine
{
    public abstract class CoroutineInstruct
    {
        public abstract bool IsDone();
    }

    public class WaitForSeconds : CoroutineInstruct
    {
        private double mWaitSeconds;
        private double mStartTime;

        public WaitForSeconds(double seconds)
        {
            mWaitSeconds = seconds;
            mStartTime = (double)DateTime.Now.Ticks / (1000.0 * 10000.0);
        }

        public override bool IsDone()
        {
            return (double)DateTime.Now.Ticks / (1000.0 * 10000.0) - mStartTime >= mWaitSeconds;
        }
    }

    public class WaitForEvent : CoroutineInstruct
    {
        AutoResetEvent mAutoEvent;
        private double mWaitSeconds;
        private double mStartTime;

        public WaitForEvent(AutoResetEvent autoEvent, double seconds)
        {
            mAutoEvent = autoEvent;

            mWaitSeconds = seconds;
            mStartTime = DateTime.Now.Ticks / (1000 * 10000);
        }

        public override bool IsDone()
        {
            return mAutoEvent.WaitOne(0) || DateTime.Now.Ticks / (1000 * 10000) - mStartTime >= mWaitSeconds;
        }
    }

    public class WaitForFrame : CoroutineInstruct
    {
        public WaitForFrame()
        {
        }

        public override bool IsDone()
        {
            return true;
        }
    }

    public class Coroutine : CoroutineInstruct
    {
        public IEnumerator enumerator;
        public bool removed = false;
        public Coroutine(IEnumerator _enumerator)
        {
            enumerator = _enumerator;
        }

        public override bool IsDone()
        {
            return !CoroutineManager.Instance.ExisCoroutine(this);
        }
    }

    public class CoroutineGroup : CoroutineInstruct
    {
        public Coroutine[] coroutines;
        public CoroutineGroup(Coroutine[] _coroutines)
        {
            coroutines = _coroutines;
        }

        public override bool IsDone()
        {
            foreach (var coroutine in coroutines)
            {
                if (CoroutineManager.Instance.ExisCoroutine(coroutine))
                    return false;
            }

            return true;
        }
    }

    public sealed class CoroutineManager
    {
        public readonly static CoroutineManager Instance = new CoroutineManager();

        List<Coroutine> mCoroutines = new List<Coroutine>();

        public CoroutineManager()
        {
        }

        public int CoroutineCount
        {
            get { return mCoroutines.Count; }
        }

        public void Update()
        {
            int i = 0;
            int count = mCoroutines.Count;
            while (i < count)
            {
                if (mCoroutines[i].removed)
                {
                    mCoroutines.RemoveAt(i);
                    count--;
                    continue;
                }

                if (mCoroutines[i].enumerator.Current is CoroutineInstruct)
                {
                    CoroutineInstruct yieldInstruction = mCoroutines[i].enumerator.Current as CoroutineInstruct;
                    if (!yieldInstruction.IsDone())
                    {
                        i++;
                        continue;
                    }
                }

                if (!mCoroutines[i].enumerator.MoveNext())
                {
                    mCoroutines.RemoveAt(i);
                    count--;
                    continue;
                }

                i++;
            }
        }

        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            Coroutine coroutine = new Coroutine(enumerator);

            mCoroutines.Add(coroutine);

            return coroutine;
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            coroutine.removed = true;
        }

        public bool ExisCoroutine(Coroutine coroutine)
        {
            return mCoroutines.Exists(x => x == coroutine);
        }
    }
}

using System.Runtime.CompilerServices;

namespace CraftNet.Services;

public partial class TimerWheel
{
    class TimerSlot
    {
        public readonly  int               Level;
        public readonly  TimerLinkedList[] Timers;
        private readonly uint              _mask = 0;
        private readonly byte              _rshBit;

        public TimerSlot(int level, int count, uint mask, byte rshBit, ITimerService timerService)
        {
            this.Level = level;
            Timers     = new TimerLinkedList[count];
            for (int i = 0; i < count; i++)
                Timers[i] = new TimerLinkedList(timerService);
            _mask   = mask;
            _rshBit = rshBit;
        }

        public void Add(TimerNode timer)
        {
            byte index = GetIndex((uint)timer.Expires);
            var  list  = Timers[index];
            list.AddLast(timer);

            //Log.Debug($"timer add: level={Level} index={index} expires={timer.expires}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetIndex(uint time)
        {
            uint v = (time >> _rshBit) & _mask;
            return (byte)v;
        }

        public void MoveTo(byte src, TimerSlot dstSlot)
        {
            TimerLinkedList list = Timers[src];
            while (list.First != null)
            {
                TimerNode timer = list.First;
                list.Remove(timer);
                dstSlot.Add(timer);
            }
        }
    }
}
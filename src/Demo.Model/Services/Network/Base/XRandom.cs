namespace Chuan
{
    public struct XRandom
    {
        public static XRandom Default = new XRandom(0);

        ulong?      state; // = 0x4d595df4d0f33173; // Or something seed-dependent
        const ulong multiplier = 6364136223846793005u;
        const ulong increment  = 1442695040888963407u; // Or an arbitrary odd constant

        static uint rotr32(uint x, int r)
        {
            return x >> r | x << (-r & 31);
        }

        public uint RandomInt()
        {
            if (!state.HasValue)
                return 0;
            ulong x     = state.Value;
            int   count = (int)(x >> 59); // 59 = 64 - 5
            state =  x * multiplier + increment;
            x     ^= x >> 18;                      // 18 = (64 - 27)/2
            return rotr32((uint)(x >> 27), count); // 27 = 32 - 5
        }

        public XRandom(ulong? seed)
        {
            state = seed + increment;
        }
    }
}
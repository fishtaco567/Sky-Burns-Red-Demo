

namespace Utils {
    public static class SquirrelNoise {

        public static uint Get1DNoiseUint(int position, uint seed = 0) {
            const uint BIT_NOISE1 = 0x68E31DA4;
            const uint BIT_NOISE2 = 0x85297A4D;
            const uint BIT_NOISE3 = 0x1B56C4E9;

            unchecked {
                uint mangledBits = (uint) position;
                mangledBits *= BIT_NOISE1;
                mangledBits += seed;
                mangledBits ^= (mangledBits >> 8);
                mangledBits += BIT_NOISE2;
                mangledBits ^= (mangledBits << 8);
                mangledBits *= BIT_NOISE3;
                mangledBits ^= (mangledBits >> 8);

                return mangledBits;
            }
        }

        public static uint Get2DNoiseUint(int posX, int posY, uint seed = 0) {
            return unchecked(Get1DNoiseUint(posX + posY * 27742151, seed));
        }

        public static uint Get3DNoiseUint(int posX, int posY, int posZ, uint seed = 0) {
            return unchecked(Get1DNoiseUint(posX + posY * 27833021 + posZ * 317130731, seed));
        }

        public static uint Get4DNoiseUint(int posX, int posY, int posZ, int posW, uint seed = 0) {
            return unchecked(Get1DNoiseUint(posX + posY * 29399999 + posZ * 325767523 + posW * 1495052261, seed));
        }
        public static float Get1DNoiseZeroToOne(int position, uint seed = 0) {
            return unchecked((Get1DNoiseUint(position, seed) - 1f) / (uint.MaxValue));
        }

        public static float Get2DNoiseZeroToOne(int posX, int posY, uint seed = 0) {
            return unchecked(Get1DNoiseUint(posX + posY * 27742151, seed) / (uint.MaxValue + 1f));
        }

        public static float Get3DNoiseZeroToOne(int posX, int posY, int posZ, uint seed = 0) {
            return unchecked(Get1DNoiseUint(posX + posY * 27833021 + posZ * 317130731, seed) / (uint.MaxValue + 1f));
        }

        public static float Get4DNoiseZeroToOne(int posX, int posY, int posZ, int posW, uint seed = 0) {
            return unchecked(Get1DNoiseUint(posX + posY * 29399999 + posZ * 325767523 + posW * 1495052261, seed) / (uint.MaxValue + 1f));
        }
        public static float Get1DNoiseNegativeOneToOne(int position, uint seed = 0) {
            return unchecked((Get1DNoiseUint(position, seed) / (float) uint.MaxValue) * 2 - 1);
        }

        public static float Get2DNoiseNegativeToOne(int posX, int posY, uint seed = 0) {
            return unchecked((Get1DNoiseUint(posX + posY * 27742151, seed) / (float)uint.MaxValue) * 2 - 1);
        }

        public static float Get3DNoiseNegativeToOne(int posX, int posY, int posZ, uint seed = 0) {
            return unchecked((Get1DNoiseUint(posX + posY * 27833021 + posZ * 317130731, seed) / (float)uint.MaxValue) * 2 - 1);
        }

        public static float Get4DNoiseNegativeToOne(int posX, int posY, int posZ, int posW, uint seed = 0) {
            return unchecked((Get1DNoiseUint(posX + posY * 29399999 + posZ * 325767523 + posW * 1495052261, seed) / (float)uint.MaxValue) * 2 - 1);
        }

        public static int Get1DNoiseInt(int position, uint seed = 0) {
            return unchecked((int) Get1DNoiseUint(position, seed));
        }

    }
}

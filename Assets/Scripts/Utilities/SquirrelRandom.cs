using System;

namespace Utils {
    public class SRandom {

        private uint seed;
        private int position;

        public SRandom(uint seed, int position = 0) {
            this.seed = seed;
            this.position = position;
        }

        public void ResetSeed(uint seed, int position = 0) {
            this.seed = seed;
            this.position = position;
        }

        public uint GetSeed() {
            return seed;
        }

        public void SetCurrentPosition(int position) {
            this.position = position;
        }

        public int GetCurrentPosition() {
            return position;
        }

        public uint RandomUInt() {
            return SquirrelNoise.Get1DNoiseUint(position++, seed);
        }

        public int RandomInt() {
            return unchecked((int) SquirrelNoise.Get1DNoiseUint(position++, seed));
        }

        public int RandomIntLessThan(int lessThan) {
            if(lessThan == 0) {
                return 0;
            }
            int thing = (int)Math.Floor(SquirrelNoise.Get1DNoiseZeroToOne(position++, seed) * lessThan);
            return thing % lessThan;
        }

        public int RandomIntInRange(int lowerInclusive, int upperInclusive) {
            return (int)Math.Floor(SquirrelNoise.Get1DNoiseZeroToOne(position++, seed) * (upperInclusive - lowerInclusive + 1) + lowerInclusive);
        }

        public float RandomFloatZeroToOne() {
            return SquirrelNoise.Get1DNoiseZeroToOne(position++, seed);
        }

        public float RandomFloatNegativeOneToOne() {
            return SquirrelNoise.Get1DNoiseNegativeOneToOne(position++, seed);
        }

        public float RandomFloatInRange(float lowerInclusive, float upperInclusive) {
            return (SquirrelNoise.Get1DNoiseNegativeOneToOne(position++, seed) + 1) * ((upperInclusive - lowerInclusive) / 2) + lowerInclusive;
        }

        public bool RandomChance(float probabilityTrue) {
            return SquirrelNoise.Get1DNoiseZeroToOne(position++, seed) < probabilityTrue;
        }

        public UnityEngine.Vector2 RandomInCircle(float radius) {
            float x = RandomFloatInRange(-radius, radius);
            float y = RandomFloatInRange(-radius, radius);
            float r2 = radius * radius;

            while(x * x + y * y > r2) {
                x = RandomFloatInRange(-radius, radius);
                y = RandomFloatInRange(-radius, radius);
            }

            return new UnityEngine.Vector2(x, y);
        }

    }
}

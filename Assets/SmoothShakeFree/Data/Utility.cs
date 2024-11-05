namespace SmoothShakeFree
{
    public static class Utility
    {
        public static float Remap(float valueInput, float oldRangeMin, float oldRangeMax, float newRangeMin, float newRangeMax)
        {
            return newRangeMin + (valueInput - oldRangeMin) * (newRangeMax - newRangeMin) / (oldRangeMax - oldRangeMin);
        }
    }
}

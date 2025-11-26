namespace ShopOwnerSimulator.Services
{
    // 가격 변동 알고리즘 스텁
    // 실제 알고리즘은 요구사항에 맞춰 구현하세요.
    public static class PriceAlgorithm
    {
        /// <summary>
        /// 다음 시세를 계산합니다. 현재는 단순히 현재값을 반환하는 플레이스홀더입니다.
        /// </summary>
        public static decimal CalculateNextPrice(decimal currentPrice, decimal volatility = 0.0m)
        {
            // TODO: 실제 변동 로직 구현 (랜덤, 모멘텀, 거래량 반영 등)
            return currentPrice;
        }
    }
}

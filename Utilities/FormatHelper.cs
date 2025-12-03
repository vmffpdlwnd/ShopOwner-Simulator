namespace ShopOwnerSimulator.Utils
{
	public static class FormatHelper
	{
		public static string FormatCurrency(decimal v) => v.ToString("C");

		public static string GetItemDisplayName(string templateId)
		{
			if (string.IsNullOrEmpty(templateId)) return "알 수 없는 아이템";

			return templateId switch
			{
				"material_ore" => "광석",
				"material_wood" => "목재",
				"material_herb" => "약초",
				"equipment_sword" => "철검",
				"equipment_armor" => "가죽 갑옷",
				"consumable_potion" => "회복 물약",
				_ => templateId
			};
		}
		public static string GetItemDescription(string templateId)
		{
			if (string.IsNullOrEmpty(templateId)) return "설명이 없습니다";

			return templateId switch
			{
				"material_ore" => "철과 합금의 재료",
				"material_wood" => "건축과 제작에 사용하는 자재",
				"material_herb" => "물약 제작에 필요한 재료",
				"equipment_sword" => "초보자용 근접 무기",
				"equipment_armor" => "가벼운 방어구",
				"consumable_potion" => "HP를 회복시킵니다",
				_ => "설명이 없습니다"
			};
		}
	}
}

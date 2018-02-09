using System.Collections.Generic;
using Yuka.Script;

namespace Yuka {
	class Constants {
		public static readonly byte[] PNG_MAGIC = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };
		public static readonly byte[] YKG_MAGIC = { 0x89, 0x47, 0x4e, 0x50, 0x0d, 0x0a, 0x1a, 0x0a };
		public static readonly byte[] YKG_HEADER = { 0x59, 0x4b, 0x47, 0x30, 0x30, 0x30, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; //, 0x40, 0x00, 0x00, 0x00, 0xa4, 0x01, 0x00, 0x00, 0xe4, 0x01, 0x00, 0x00, 0xb3, 0x01, 0x00, 0x00, 0x97, 0x03, 0x00, 0x00, 0x20, 0x01, 0x00, 0x00 };

		public const string yks = "yks";
		public const string ykd = "ykd";
		public const string csv = "csv";

		public const string ykg = "ykg";
		public const string png = "png";
		public const string meta = "meta";

		public const string ykc = "ykc";
		public const string ini = "ini";

		public const string ogg = "ogg";

		public const string ypl = "ypl";
		public const string ydr = "ydr";

		public const string html = "html";

		public static Dictionary<string, string> knownVars = new Dictionary<string, string>();

		public static TextUtils.FontMetrics[] fontInfo = new[] {
			new TextUtils.FontMetrics(24, 32),
			new TextUtils.FontMetrics(24),
			null,
			new TextUtils.FontMetrics(10),
			new TextUtils.FontMetrics(30),
			new TextUtils.FontMetrics(36),
			new TextUtils.FontMetrics(50),
			new TextUtils.FontMetrics(20),
			new TextUtils.FontMetrics(14),
			new TextUtils.FontMetrics(22, 32),
			new TextUtils.FontMetrics(24, 40),
			null, null, null, null, null
		};

		static Constants() {
			knownVars["flag_0"] = "flag_$";
			knownVars["globalflag_0"] = "globalflag_$";
			knownVars["str_0"] = "str_$";
			knownVars["globalstr_0"] = "globalstr_$";

			knownVars["flag_1"] = "autosave_valid";

			knownVars["flag_10"] = "slider_position";

			// knownVars["flag_21"] = "channel_id"; // used in multiple contexts
			knownVars["flag_22"] = "channel_volume";

			knownVars["flag_31"] = "id_chinatsu";
			knownVars["flag_32"] = "id_kaho";
			knownVars["flag_33"] = "id_tsugumi";
			knownVars["flag_34"] = "id_nanako";
			knownVars["flag_35"] = "id_satsuki";

			knownVars["flag_92"] = "effect_shake_x_1";
			knownVars["flag_93"] = "effect_shake_y_1";
			knownVars["flag_94"] = "effect_shake_x_2";
			knownVars["flag_95"] = "effect_shake_y_2";
			knownVars["flag_96"] = "effect_shake_offset";
			knownVars["flag_97"] = "effect_shake_delay";

			knownVars["flag_100"] = "selected";
			knownVars["flag_101"] = "choice_count";
			knownVars["flag_110"] = "yes_no_answer";

			knownVars["flag_190"] = "display_bonus_menu";
			knownVars["flag_191"] = "indicate_bonus_menu";
			knownVars["flag_199"] = "play_sound_on_exit";

			knownVars["flag_205"] = "sound_enabled";
			knownVars["flag_210"] = "textbox_visible";
			knownVars["flag_230"] = "h_font_mode";
			knownVars["flag_235"] = "textbox_force_hidden";
			knownVars["flag_240"] = "settings_page";

			knownVars["flag_261"] = "skip_unread_mode";
			knownVars["flag_262"] = "skip_all_mode";
			knownVars["flag_263"] = "auto_mode";

			knownVars["flag_280"] = "replay_mode";
			knownVars["flag_285"] = "non_default_player_name";
			knownVars["flag_290"] = "save_menu_enabled";

			knownVars["flag_300"] = "selected_route";
			knownVars["flag_499"] = "nude_mode";

			knownVars["flag_600"] = "hairstyle_chinatsu";
			knownVars["flag_610"] = "clothes_chinatsu";

			knownVars["flag_700"] = "hairstyle_kaho";
			knownVars["flag_710"] = "clothes_kaho";

			knownVars["flag_800"] = "hairstyle_tsugumi";
			knownVars["flag_810"] = "clothes_tsugumi";

			knownVars["flag_900"] = "hairstyle_nanako";
			knownVars["flag_910"] = "clothes_nanako";

			knownVars["flag_1000"] = "hairstyle_satsuki";
			knownVars["flag_1010"] = "clothes_satsuki";

			knownVars["flag_1100"] = "hairstyle_current";
			knownVars["flag_1110"] = "clothes_current";

			knownVars["flag_1500"] = "affection_chinatsu";
			knownVars["flag_1510"] = "affection_kaho";
			knownVars["flag_1520"] = "affection_tsugumi";
			knownVars["flag_1530"] = "affection_nanako";
			knownVars["flag_1540"] = "affection_satsuki";

			knownVars["flag_2000"] = "button_row_1_col_1";
			knownVars["flag_2001"] = "button_row_1_col_2";
			knownVars["flag_2002"] = "button_row_1_col_3";
			knownVars["flag_2010"] = "button_row_2_col_1";
			knownVars["flag_2011"] = "button_row_2_col_2";
			knownVars["flag_2012"] = "button_row_2_col_3";
			knownVars["flag_2020"] = "button_row_3_col_1";
			knownVars["flag_2021"] = "button_row_3_col_2";
			knownVars["flag_2022"] = "button_row_3_col_3";
			knownVars["flag_2030"] = "button_row_4_col_1";
			knownVars["flag_2031"] = "button_row_4_col_2";
			knownVars["flag_2032"] = "button_row_4_col_3";
			//knownVars["flag_20"] = "";

			knownVars["flag_4000"] = "save_load_page";
			knownVars["flag_4001"] = "save_load_slot";
			knownVars["flag_4002"] = "save_load_id";
			knownVars["flag_4003"] = "save_load_id_valid";

			knownVars["flag_4100"] = "save_load_page_1_button_enabled";
			knownVars["flag_4101"] = "save_load_page_2_button_enabled";
			knownVars["flag_4102"] = "save_load_page_3_button_enabled";
			knownVars["flag_4103"] = "save_load_page_4_button_enabled";
			knownVars["flag_4104"] = "save_load_page_5_button_enabled";
			knownVars["flag_4105"] = "save_load_page_6_button_enabled";
			knownVars["flag_4106"] = "save_load_page_7_button_enabled";
			knownVars["flag_4107"] = "save_load_page_8_button_enabled";
			knownVars["flag_4108"] = "save_load_page_9_button_enabled";
			knownVars["flag_4109"] = "save_load_page_10_button_enabled";
			knownVars["flag_4110"] = "save_load_page_11_button_enabled";
			knownVars["flag_4111"] = "save_load_page_12_button_enabled";
			knownVars["flag_4112"] = "save_load_page_13_button_enabled";
			knownVars["flag_4113"] = "save_load_page_14_button_enabled";
			knownVars["flag_4114"] = "save_load_page_15_button_enabled";
			knownVars["flag_4115"] = "save_load_page_16_button_enabled";
			knownVars["flag_4116"] = "save_load_page_17_button_enabled";
			knownVars["flag_4117"] = "save_load_page_18_button_enabled";
			knownVars["flag_4118"] = "save_load_page_19_button_enabled";
			knownVars["flag_4119"] = "save_load_page_20_button_enabled";

			knownVars["flag_5000"] = "bonus_page";

			knownVars["flag_5010"] = "bonus_tab_chinatsu_enabled";
			knownVars["flag_5011"] = "bonus_tab_kaho_enabled";
			knownVars["flag_5012"] = "bonus_tab_tsugumi_enabled";
			knownVars["flag_5013"] = "bonus_tab_nanako_enabled";
			knownVars["flag_5014"] = "bonus_tab_satsuki_enabled";

			knownVars["flag_5101"] = "bonus_cg_1_enabled";
			knownVars["flag_5102"] = "bonus_cg_2_enabled";
			knownVars["flag_5103"] = "bonus_cg_3_enabled";
			knownVars["flag_5104"] = "bonus_cg_4_enabled";
			knownVars["flag_5105"] = "bonus_cg_5_enabled";
			knownVars["flag_5106"] = "bonus_cg_6_enabled";
			knownVars["flag_5107"] = "bonus_cg_7_enabled";
			knownVars["flag_5108"] = "bonus_cg_8_enabled";
			knownVars["flag_5109"] = "bonus_cg_9_enabled";
			knownVars["flag_5110"] = "bonus_cg_10_enabled";
			knownVars["flag_5111"] = "bonus_cg_11_enabled";
			knownVars["flag_5112"] = "bonus_cg_12_enabled";
			knownVars["flag_5113"] = "bonus_cg_13_enabled";
			knownVars["flag_5114"] = "bonus_cg_14_enabled";
			knownVars["flag_5115"] = "bonus_cg_15_enabled";
			knownVars["flag_5116"] = "bonus_cg_16_enabled";
			knownVars["flag_5117"] = "bonus_cg_17_enabled";

			knownVars["flag_5120"] = "bonus_scene_1_enabled";
			knownVars["flag_5121"] = "bonus_scene_2_enabled";
			knownVars["flag_5122"] = "bonus_scene_3_enabled";
			knownVars["flag_5123"] = "bonus_scene_4_enabled";
			knownVars["flag_5124"] = "bonus_scene_5_enabled";

			knownVars["flag_5130"] = "bonus_hairstyle_1_enabled";
			knownVars["flag_5131"] = "bonus_hairstyle_2_enabled";

			knownVars["flag_5140"] = "bonus_clothes_1_enabled";
			knownVars["flag_5141"] = "bonus_clothes_2_enabled";

			knownVars["flag_5200"] = "bonus_current_cg";
			knownVars["flag_5210"] = "bonus_current_scene";

			knownVars["flag_5230"] = "bonus_hairstyle_1";
			knownVars["flag_5231"] = "bonus_hairstyle_2";

			knownVars["flag_5240"] = "bonus_clothes_1";
			knownVars["flag_5241"] = "bonus_clothes_2";

			knownVars["flag_5510"] = "bonus_current_hairstyle";
			knownVars["flag_5520"] = "bonus_current_clothes";

			knownVars["flag_6000"] = "bonus_music_current_track";

			// knownVars["globalflag_"] = "";

			knownVars["globalflag_2"] = "settings_font";
			knownVars["globalflag_3"] = "settings_skip_unread";
			knownVars["globalflag_4"] = "settings_text_speed";
			knownVars["globalflag_5"] = "settings_auto_speed";
			knownVars["globalflag_10"] = "settings_effect_quality";
			knownVars["globalflag_11"] = "settings_rightclick_action";
			knownVars["globalflag_12"] = "settings_continue_auto";
			knownVars["globalflag_13"] = "settings_continue_skip";
			knownVars["globalflag_14"] = "settings_font_effect";
			knownVars["globalflag_15"] = "settings_textbox_opacity";
			knownVars["globalflag_16"] = "settings_stop_sound_on_click";

			knownVars["globalflag_30"] = "text_color_red";
			knownVars["globalflag_31"] = "text_color_green";
			knownVars["globalflag_32"] = "text_color_blue";

			knownVars["globalflag_33"] = "text_border_color_red";
			knownVars["globalflag_34"] = "text_border_color_green";
			knownVars["globalflag_35"] = "text_border_color_blue";

			knownVars["globalflag_51"] = "show_logo"; // skip_logo

			knownVars["globalflag_85"] = "save_load_page_global";
			knownVars["globalflag_86"] = "save_load_slot_global";

			knownVars["globalflag_100"] = "settings_volume_music";
			knownVars["globalflag_101"] = "settings_volume_effect";
			knownVars["globalflag_102"] = "settings_volume_master";
			knownVars["globalflag_103"] = "settings_volume_female";
			knownVars["globalflag_104"] = "settings_volume_male";
			knownVars["globalflag_110"] = "settings_volume_chinatsu";
			knownVars["globalflag_111"] = "settings_volume_kaho";
			knownVars["globalflag_112"] = "settings_volume_tsugumi";
			knownVars["globalflag_113"] = "settings_volume_nanako";
			knownVars["globalflag_114"] = "settings_volume_satsuki";
			knownVars["globalflag_115"] = "settings_volume_mifuyu";
			knownVars["globalflag_116"] = "settings_volume_nao";
			knownVars["globalflag_117"] = "settings_volume_kie";
			knownVars["globalflag_118"] = "settings_volume_manager";
			knownVars["globalflag_119"] = "settings_volume_ryuusei";
			knownVars["globalflag_120"] = "settings_volume_other_female";
			knownVars["globalflag_121"] = "settings_volume_other_male";

			knownVars["globalflag_150"] = "prologue_finished";
			knownVars["globalflag_200"] = "route_finished";

			knownVars["globalflag_220"] = "cg_unlocked_chinatsu_1";
			knownVars["globalflag_221"] = "cg_unlocked_chinatsu_2";
			knownVars["globalflag_222"] = "cg_unlocked_chinatsu_3";
			knownVars["globalflag_223"] = "cg_unlocked_chinatsu_4";
			knownVars["globalflag_224"] = "cg_unlocked_chinatsu_5";
			knownVars["globalflag_225"] = "cg_unlocked_chinatsu_6";
			knownVars["globalflag_226"] = "cg_unlocked_chinatsu_7";
			knownVars["globalflag_227"] = "cg_unlocked_chinatsu_8";
			knownVars["globalflag_228"] = "cg_unlocked_chinatsu_9";
			knownVars["globalflag_229"] = "cg_unlocked_chinatsu_10";
			knownVars["globalflag_230"] = "cg_unlocked_chinatsu_11";
			knownVars["globalflag_231"] = "cg_unlocked_chinatsu_12";
			knownVars["globalflag_232"] = "cg_unlocked_chinatsu_13";
			knownVars["globalflag_233"] = "cg_unlocked_chinatsu_14";
			knownVars["globalflag_234"] = "cg_unlocked_chinatsu_15";
			knownVars["globalflag_235"] = "cg_unlocked_chinatsu_16";
			knownVars["globalflag_236"] = "cg_unlocked_chinatsu_17";

			knownVars["globalflag_250"] = "cg_unlocked_kaho_1";
			knownVars["globalflag_251"] = "cg_unlocked_kaho_2";
			knownVars["globalflag_252"] = "cg_unlocked_kaho_3";
			knownVars["globalflag_253"] = "cg_unlocked_kaho_4";
			knownVars["globalflag_254"] = "cg_unlocked_kaho_5";
			knownVars["globalflag_255"] = "cg_unlocked_kaho_6";
			knownVars["globalflag_256"] = "cg_unlocked_kaho_7";
			knownVars["globalflag_257"] = "cg_unlocked_kaho_8";
			knownVars["globalflag_258"] = "cg_unlocked_kaho_9";
			knownVars["globalflag_259"] = "cg_unlocked_kaho_10";
			knownVars["globalflag_260"] = "cg_unlocked_kaho_11";
			knownVars["globalflag_261"] = "cg_unlocked_kaho_12";
			knownVars["globalflag_262"] = "cg_unlocked_kaho_13";
			knownVars["globalflag_263"] = "cg_unlocked_kaho_14";
			knownVars["globalflag_264"] = "cg_unlocked_kaho_15";
			knownVars["globalflag_265"] = "cg_unlocked_kaho_16";
			knownVars["globalflag_266"] = "cg_unlocked_kaho_17";

			knownVars["globalflag_280"] = "cg_unlocked_tsugumi_1";
			knownVars["globalflag_281"] = "cg_unlocked_tsugumi_2";
			knownVars["globalflag_282"] = "cg_unlocked_tsugumi_3";
			knownVars["globalflag_283"] = "cg_unlocked_tsugumi_4";
			knownVars["globalflag_284"] = "cg_unlocked_tsugumi_5";
			knownVars["globalflag_285"] = "cg_unlocked_tsugumi_6";
			knownVars["globalflag_286"] = "cg_unlocked_tsugumi_7";
			knownVars["globalflag_287"] = "cg_unlocked_tsugumi_8";
			knownVars["globalflag_288"] = "cg_unlocked_tsugumi_9";
			knownVars["globalflag_289"] = "cg_unlocked_tsugumi_10";
			knownVars["globalflag_290"] = "cg_unlocked_tsugumi_11";
			knownVars["globalflag_291"] = "cg_unlocked_tsugumi_12";
			knownVars["globalflag_292"] = "cg_unlocked_tsugumi_13";
			knownVars["globalflag_293"] = "cg_unlocked_tsugumi_14";
			knownVars["globalflag_294"] = "cg_unlocked_tsugumi_15";
			knownVars["globalflag_295"] = "cg_unlocked_tsugumi_16";
			knownVars["globalflag_296"] = "cg_unlocked_tsugumi_17";

			knownVars["globalflag_310"] = "cg_unlocked_nanako_1";
			knownVars["globalflag_311"] = "cg_unlocked_nanako_2";
			knownVars["globalflag_312"] = "cg_unlocked_nanako_3";
			knownVars["globalflag_313"] = "cg_unlocked_nanako_4";
			knownVars["globalflag_314"] = "cg_unlocked_nanako_5";
			knownVars["globalflag_315"] = "cg_unlocked_nanako_6";
			knownVars["globalflag_316"] = "cg_unlocked_nanako_7";
			knownVars["globalflag_317"] = "cg_unlocked_nanako_8";
			knownVars["globalflag_318"] = "cg_unlocked_nanako_9";
			knownVars["globalflag_319"] = "cg_unlocked_nanako_10";
			knownVars["globalflag_320"] = "cg_unlocked_nanako_11";
			knownVars["globalflag_321"] = "cg_unlocked_nanako_12";
			knownVars["globalflag_322"] = "cg_unlocked_nanako_13";
			knownVars["globalflag_323"] = "cg_unlocked_nanako_14";
			knownVars["globalflag_324"] = "cg_unlocked_nanako_15";
			knownVars["globalflag_325"] = "cg_unlocked_nanako_16";
			knownVars["globalflag_326"] = "cg_unlocked_nanako_17";

			knownVars["globalflag_340"] = "cg_unlocked_satsuki_1";
			knownVars["globalflag_341"] = "cg_unlocked_satsuki_2";
			knownVars["globalflag_342"] = "cg_unlocked_satsuki_3";
			knownVars["globalflag_343"] = "cg_unlocked_satsuki_4";
			knownVars["globalflag_344"] = "cg_unlocked_satsuki_5";
			knownVars["globalflag_345"] = "cg_unlocked_satsuki_6";
			knownVars["globalflag_346"] = "cg_unlocked_satsuki_7";
			knownVars["globalflag_347"] = "cg_unlocked_satsuki_8";
			knownVars["globalflag_348"] = "cg_unlocked_satsuki_9";
			knownVars["globalflag_349"] = "cg_unlocked_satsuki_10";
			knownVars["globalflag_350"] = "cg_unlocked_satsuki_11";
			knownVars["globalflag_351"] = "cg_unlocked_satsuki_12";
			knownVars["globalflag_352"] = "cg_unlocked_satsuki_13";
			knownVars["globalflag_353"] = "cg_unlocked_satsuki_14";
			knownVars["globalflag_354"] = "cg_unlocked_satsuki_15";
			knownVars["globalflag_355"] = "cg_unlocked_satsuki_16";
			knownVars["globalflag_356"] = "cg_unlocked_satsuki_17";

			knownVars["globalflag_450"] = "scene_unlocked_chinatsu_1";
			knownVars["globalflag_451"] = "scene_unlocked_chinatsu_2";
			knownVars["globalflag_452"] = "scene_unlocked_chinatsu_3";
			knownVars["globalflag_453"] = "scene_unlocked_chinatsu_4";
			knownVars["globalflag_454"] = "scene_unlocked_chinatsu_5";

			knownVars["globalflag_460"] = "scene_unlocked_kaho_1";
			knownVars["globalflag_461"] = "scene_unlocked_kaho_2";
			knownVars["globalflag_462"] = "scene_unlocked_kaho_3";
			knownVars["globalflag_463"] = "scene_unlocked_kaho_4";
			knownVars["globalflag_464"] = "scene_unlocked_kaho_5";

			knownVars["globalflag_470"] = "scene_unlocked_tsugumi_1";
			knownVars["globalflag_471"] = "scene_unlocked_tsugumi_2";
			knownVars["globalflag_472"] = "scene_unlocked_tsugumi_3";
			knownVars["globalflag_473"] = "scene_unlocked_tsugumi_4";
			knownVars["globalflag_474"] = "scene_unlocked_tsugumi_5";

			knownVars["globalflag_480"] = "scene_unlocked_nanako_1";
			knownVars["globalflag_481"] = "scene_unlocked_nanako_2";
			knownVars["globalflag_482"] = "scene_unlocked_nanako_3";
			knownVars["globalflag_483"] = "scene_unlocked_nanako_4";
			knownVars["globalflag_484"] = "scene_unlocked_nanako_5";

			knownVars["globalflag_490"] = "scene_unlocked_satsuki_1";
			knownVars["globalflag_491"] = "scene_unlocked_satsuki_2";
			knownVars["globalflag_492"] = "scene_unlocked_satsuki_3";
			knownVars["globalflag_493"] = "scene_unlocked_satsuki_4";
			knownVars["globalflag_494"] = "scene_unlocked_satsuki_5";

			knownVars["globalflag_500"] = "menu_transition";

			knownVars["str_1"] = "choice_text_1";
			knownVars["str_2"] = "choice_text_2";
			knownVars["str_3"] = "choice_text_3";
			knownVars["str_4"] = "choice_text_4";
			knownVars["str_5"] = "choice_text_5";

			knownVars["str_10"] = "window_title";

			knownVars["str_15"] = "save_slot_note";

			knownVars["str_18"] = "temp_name";

			knownVars["str_20"] = "player_last_name";
			knownVars["str_21"] = "player_first_name";
			knownVars["str_22"] = "player_full_name";

			knownVars["globalstr_0"] = "game_title";
			knownVars["globalstr_1"] = "game_version";

			knownVars["globalstr_20"] = "player_last_name_global";
			knownVars["globalstr_21"] = "player_first_name_global";
			knownVars["globalstr_22"] = "player_full_name_global";
		}
	}
}

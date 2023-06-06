using osb.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osb.Helpers
{
    public class DummyHelper
    {
        private static List<StoryboarderModel>? _storyboarderModelsCache;
        
        public static IEnumerable<StoryboarderModel> GenerateStoryboarders()
        {
            return _storyboarderModelsCache ??= new List<StoryboarderModel>
            {
                new(
                    12018102,
                    "EvT",
                    "https://assets.ppy.sh/user-profile-covers/12018102/95096343946dc1b07f0691e854069eb79594c745267263985d62acf3dedafd1a.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.OSBWebDeveloper
                    },
                    new CountryModel { Code = "RU", Name = "Russian Federation" }
                ),
                new(
                    3031177,
                    "Mamat",
                    "https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.OSBWebDeveloper
                    },
                    new CountryModel { Code = "ID", Name = "Indonesia" }
                ),
                new(
                    989377,
                    "Damnae",
                    "https://assets.ppy.sh/user-profile-covers/989377/6a3c9a17673ce09f8f7232d5201d024fa2a9c6419e50af9e278e76fab69c3416.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Mentor,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "FR", Name = "France" }
                ),
                new(
                    4610047,
                    "PoNo",
                    "https://assets.ppy.sh/user-profile-covers/4610047/d1cef07ea10d26f08ed7fe94c0ad1ac29066de4a9e083768a7058753b0faf220.gif",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Mentor,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "FR", Name = "France" }
                ),
                new(
                    6607303,
                    "Hokichi",
                    "https://assets.ppy.sh/user-profile-covers/6607303/b0c3db724086af998522599ac5e5938a4dbbcb86141de59b26783ee90525edcd.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Moderator,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    },
                    new CountryModel { Code = "VN", Name = "Vietnam" }
                ),
                new(
                    8286416,
                    "Marblelemons",
                    "https://assets.ppy.sh/user-profile-covers/8286416/59bed3f86265b85d850502bc80390fffc20298c75d9c51ca1e1e13b132057fb4.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Moderator,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    },
                    new CountryModel { Code = "CA", Name = "Canada" }
                ),
                new(
                    8500334,
                    "Ningguang",
                    "https://assets.ppy.sh/user-profile-covers/8500334/52105d64f131bac270fa98ddb069e4c1f6fcf289a323fd45e8fe88a527dd7f6d.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Moderator,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    },
                    new CountryModel { Code = "VN", Name = "Vietnam" }
                ),
                new(
                    3482692,
                    "WildOne94",
                    "https://osu.ppy.sh/images/headers/profile-covers/c7.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Moderator,
                        DiscordRoleModel.Sidetail,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "GB", Name = "United Kingdom" }
                ),
                new(
                    1103522,
                    "BeatofIke",
                    "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Alumni,
                        DiscordRoleModel.Sidetail,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    7279762,
                    "Coppertine",
                    "https://osu.ppy.sh/images/headers/profile-covers/c4.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Alumni,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "AU", Name = "Australia" }
                ),
                new(
                    3372928,
                    "Darky1",
                    "https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Alumni,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    },
                    new CountryModel { Code = "DE", Name = "Germany" }
                ),
                new(
                    392866,
                    "Endaris",
                    "https://assets.ppy.sh/user-profile-covers/392866/2f64885c31568e01880efbc0284d192aefdbfda2846c15d29c16ba68f1c95e47.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Alumni,
                        DiscordRoleModel.OSBWinner,
                        DiscordRoleModel.Sidetail,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "DE", Name = "Germany" }
                ),
                new(
                    2559349,
                    "Exile-",
                    "https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Alumni,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "PL", Name = "Poland" }
                ),
                new(
                    2757426,
                    "Jakomo73",
                    "https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Alumni,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    8129817,
                    "Naxess",
                    "https://assets.ppy.sh/user-profile-covers/8129817/b723ded20a013b63f7d3d3ddaa8daf27d3a8487d0987639b9c69a99bd41012b7.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Alumni,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "SE", Name = "Sweden" }
                ),
                new(
                    2036217,
                    "Sidetail",
                    "https://assets.ppy.sh/user-profile-covers/2036217/9e24df15e21500e218bcd672c92597c92decfb17db75257bf31c378ca9b91d8e.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Alumni,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    },
                    new CountryModel { Code = "CA", Name = "Canada" }
                ),
                new(
                    410,
                    "Starrodkirby86",
                    "https://assets.ppy.sh/user-profile-covers/410/3daf4b3b50177cd7117586eb944b11a48929adeaf46a5268dfba6600ccda8f32.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Alumni,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    1717308,
                    "Cookiemaid",
                    "https://assets.ppy.sh/user-profile-covers/1717308/99bfddcee5dc0b2f559e91c225ea6551ac708172d65463c30a6325251bf9efd2.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Contributor,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    },
                    new CountryModel { Code = "DE", Name = "Germany" }
                ),
                new(
                    10027577,
                    "- AzRaeL -",
                    "https://assets.ppy.sh/user-profile-covers/10027577/6479056b95379b6fbce4f06e15d7e1df95baa6cf1a75173b1ec71b3ec8983122.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "TW", Name = "Taiwan" }
                ),
                new(
                    4414426,
                    "-Ady",
                    "https://osu.ppy.sh/images/headers/profile-covers/c5.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "HU", Name = "Hungary" }
                ),
                new(
                    7467183,
                    "-Hwangbo",
                    "https://osu.ppy.sh/images/headers/profile-covers/c2.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "DE", Name = "Germany" }
                ),
                new(
                    2202163,
                    "-Mo-",
                    "https://assets.ppy.sh/user-profile-covers/2202163/55c4bb3e468973bcd24578528eaa6a57ebee7d780fdf40510f74755aa5096469.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "GB", Name = "United Kingdom" }
                ),
                new(
                    3664366,
                    "-Tochi",
                    "https://assets.ppy.sh/user-profile-covers/3664366/a2292b2bce7174e161c1399ac228087e4749562a84cf7c3fff043d75b3b1bd81.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "DK", Name = "Denmark" }
                ),
                new(
                    3638962,
                    "-Tynamo",
                    "https://assets.ppy.sh/user-profile-covers/3638962/7b5cbca5d2c5452f752b83e29c3e55d7936114c6beb1b0f40cb3b77bdfc92a44.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    997489,
                    "[ Kuro Usagi ]",
                    "https://osu.ppy.sh/images/headers/profile-covers/c2.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "KR", Name = "South Korea" }
                ),
                new(
                    568761,
                    "[Mahua]",
                    "https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CN", Name = "China" }
                ),
                new(
                    145871,
                    "arviejhay",
                    "https://osu.ppy.sh/images/headers/profile-covers/c4.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "PH", Name = "Philippines" }
                ),
                new(
                    2907231,
                    "Asonate",
                    "https://assets.ppy.sh/user-profile-covers/2907231/836cb1c651778d8ecd92b43f04828f8ffb84fb7a1d4717eaec399747b7a463f6.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    },
                    new CountryModel { Code = "AT", Name = "Austria" }
                ),
                new(
                    9131844,
                    "AYE1337",
                    "https://assets.ppy.sh/user-profile-covers/9131844/11f3ea3073eef6f1a5b5320a5106363978df89cfbde85e6ef4c0795a39e86a75.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "RU", Name = "Russian Federation" }
                ),
                new(
                    7393632,
                    "Bergy",
                    "https://assets.ppy.sh/user-profile-covers/7393632/8090434bc2a2ee04fef7bac4f6c255620abbe005df1f4c24128029aa42a88068.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    405449,
                    "BOUYAAA",
                    "https://assets.ppy.sh/user-profile-covers/405449/a58dd60b2f7ed472c99cb3cad6cb7e93944e76071e9d3d97aad0702a7d79c718.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "FR", Name = "France" }
                ),
                new(
                    10198015,
                    "BowLL",
                    "https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    4930630,
                    "C00L",
                    "https://assets.ppy.sh/user-profile-covers/4930630/248b294b728e04f7952ad8d1f0ee667de3379e03f71302c334341bffa5c04340.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "GB", Name = "United Kingdom" }
                ),
                new(
                    3621552,
                    "Chaoslitz",
                    "https://assets.ppy.sh/user-profile-covers/3621552/7a4c0872df350f1443a34fcecc8dc974dc1ddf0492996805165218719678c4b7.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "HK", Name = "Hong Kong" }
                ),
                new(
                    3700647,
                    "Cheesecake",
                    "https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "GB", Name = "United Kingdom" }
                ),
                new(
                    2172778,
                    "Cosmolade",
                    "https://assets.ppy.sh/user-profile-covers/2172778/db23ab044eab9b0aafc9d0a0729b2f615ca25f8c2994cc056e61e754a8373686.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CL", Name = "Chile" }
                ),
                new(
                    2675027,
                    "CSLM",
                    "https://osu.ppy.sh/images/headers/profile-covers/c4.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.OSBWinner,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Storyboarder
                    },
                    new CountryModel { Code = "VE", Name = "Venezuela" }
                ),
                new(
                    4121612,
                    "Doj",
                    "https://osu.ppy.sh/images/headers/profile-covers/c5.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    5178122,
                    "Enkrypton",
                    "https://assets.ppy.sh/user-profile-covers/5178122/fc2aa18cb4d3ff88cd17e897d995bcb3855f074668ceb4bb1946eef47e1afa24.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    },
                    new CountryModel { Code = "CA", Name = "Canada" }
                ),
                new(
                    2683648,
                    "Gaia",
                    "https://assets.ppy.sh/user-profile-covers/2683648/6a0158335783ff415ad837f02983f9fbb95a1e4f1b5c7efa733c49a45182a748.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail,
                        DiscordRoleModel.OSBWinner
                    },
                    new CountryModel { Code = "CA", Name = "Canada" }
                ),
                new(
                    23636,
                    "Gelbana",
                    "https://osu.ppy.sh/images/headers/profile-covers/c7.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "AU", Name = "Australia" }
                ),
                new(
                    7515767,
                    "Half",
                    "https://assets.ppy.sh/user-profile-covers/7515767/a1e944531dc135e0041a8d9605228767ed7c4049b0223edd11a35f58a4d3ee6b.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    },
                    new CountryModel { Code = "KR", Name = "South Korea" }
                ),
                new(
                    10959366,
                    "Himada",
                    "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "HU", Name = "Hungary" }
                ),
                new(
                    4477199,
                    "how2miss",
                    "https://assets.ppy.sh/user-profile-covers/4477199/80f02e2b120725116e13e097b95d487dc57ea158bd8ee92bde5b548cdc1f9d04.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "HK", Name = "Hong Kong" }
                ),
                new(
                    1287964,
                    "Irreversible",
                    "https://assets.ppy.sh/user-profile-covers/1287964/bfdf58bab2c132460fe685e5934de4df18f068842f0d9514049abc4b7fdea426.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CH", Name = "Switzerland" }
                ),
                new(
                    605973,
                    "Jerry",
                    "https://assets.ppy.sh/user-profile-covers/605973/03a7f60ef29dcdb666795e1b1223c0bb3d9886b3b7b572b53d11d8ce4a191b3e.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "MY", Name = "Malaysia" }
                ),
                new(
                    11334594,
                    "K4L1",
                    "https://assets.ppy.sh/user-profile-covers/11334594/605f7ae626143422b0abf5ec80e7a713dfea8b70feab8462e7ff10009fc042cc.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "BR", Name = "Brazil" }
                ),
                new(
                    6203841,
                    "Kagetsu",
                    "https://assets.ppy.sh/user-profile-covers/6203841/f7b3e14f15cc6055534899c05f9611fbe7a0766e372c54e2f258b4996b16f718.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CL", Name = "Chile" }
                ),
                new(
                    4588894,
                    "Kawa",
                    "https://assets.ppy.sh/user-profile-covers/4588894/750afb0bfc4298adfc8039b415fbf07415f56bcd095fc3699e4bcb49c10a9a4e.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "SI", Name = "Slovenia" }
                ),
                new(
                    3193504,
                    "Kibbleru",
                    "https://assets.ppy.sh/user-profile-covers/3193504/964392afa3931d51e492ecc8441420ff5d2049980a3eaf51aed47db5505778e6.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    },
                    new CountryModel { Code = "CA", Name = "Canada" }
                ),
                new(
                    6984103,
                    "Kuse",
                    "https://assets.ppy.sh/user-profile-covers/6984103/44357b8ac236de051a930f594a4b11c5a5f108a8c7af2fcb06a8b4d65b474179.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CN", Name = "China" }
                ),
                new(
                    15148343,
                    "LeeGod",
                    "https://assets.ppy.sh/user-profile-covers/15148343/a3e6bceccce515ae2989d733ebe4d91af8500cd7af8b753df147a1a1da900868.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "TW", Name = "Taiwan" }
                ),
                new(
                    7306763,
                    "LowAccuracySS",
                    "https://assets.ppy.sh/user-profile-covers/7306763/d937588921e84466024d49a14602c531b13c12636990d476c88783a2182d88fc.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    1863798,
                    "Lumy",
                    "https://assets.ppy.sh/user-profile-covers/1863798/2bd60c8c369e2db41e1c2ae28519350a99329b46814f1c8e94741bbafb1da522.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "FR", Name = "France" }
                ),
                new(
                    2204515,
                    "Mao",
                    "https://assets.ppy.sh/user-profile-covers/2204515/044f2220b73380eef623b694e432958e47163235d6570c8d0adee3edefdd8e1f.png",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "DE", Name = "Germany" }
                ),
                new(
                    3021634,
                    "Meow Mix",
                    "https://assets.ppy.sh/user-profile-covers/3021634/8d9096d5a6293bbad392ec7098dce5dca7fd2ebf472121777c584cad07d180c5.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CA", Name = "Canada" }
                ),
                new(
                    4028934,
                    "Meyrink",
                    "https://assets.ppy.sh/user-profile-covers/4028934/6d84447e1c879ace0521beea4c1ac794ac406fa48039165f293c31fa844d1d62.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified,
                        DiscordRoleModel.Sidetail
                    },
                    new CountryModel { Code = "MA", Name = "Morocco" }
                ),
                new(
                    5075660,
                    "Moecho",
                    "https://assets.ppy.sh/user-profile-covers/5075660/55160295b14be2d5e50648d04209d4cbabb2af59b9386f90a03cdcec28c420cf.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CN", Name = "China" }
                ),
                new(
                    6488658,
                    "Moko",
                    "https://assets.ppy.sh/user-profile-covers/6488658/8019e1a7f78deece7bb357daf16f0a2633a98a6d8b5780c6a1a4400c527ceab3.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "PL", Name = "Poland" }
                ),
                new(
                    2330619,
                    "Mr HeliX",
                    "https://assets.ppy.sh/user-profile-covers/2330619/009fe40fa2e365de60aa063e558a87a1ee3e6f8894ca2f19549c5193727f90eb.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "NL", Name = "Netherlands" }
                ),
                new(
                    8309500,
                    "MRL",
                    "https://osu.ppy.sh/images/headers/profile-covers/c8.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "SE", Name = "Sweden" }
                ),
                new(
                    8878806,
                    "MyAngelHelen",
                    "https://osu.ppy.sh/images/headers/profile-covers/c7.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "GB", Name = "United Kingdom" }
                ),
                new(
                    4110225,
                    "Mykaterasu",
                    "https://assets.ppy.sh/user-profile-covers/4110225/201306f0472aa699d0d0629f100316e594f03977a3c539e562eaafe96750d923.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "GB", Name = "United Kingdom" }
                ),
                new(
                    426638,
                    "nanobuilder",
                    "https://osu.ppy.sh/images/headers/profile-covers/c5.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    1557955,
                    "Nerova Riuz GX",
                    "https://osu.ppy.sh/images/headers/profile-covers/c4.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "TW", Name = "Taiwan" }
                ),
                new(
                    2747929,
                    "niseboi",
                    "https://osu.ppy.sh/images/headers/profile-covers/c4.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    7293512,
                    "Nitrous",
                    "https://assets.ppy.sh/user-profile-covers/7293512/c96ca02e02bbd7003f09ace998a2076df4496e824f5c4cc0c3985af982bc2291.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "PH", Name = "Philippines" }
                ),
                new(
                    1541323,
                    "Noffy",
                    "https://assets.ppy.sh/user-profile-covers/1541323/62f91185704780215aa92f39216b2c1265844debafc1368f6f0d56997ff7986d.png",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    4468239,
                    "ohm002",
                    "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "TH", Name = "Thailand" }
                ),
                new(
                    3683084,
                    "pandorawindy",
                    "https://osu.ppy.sh/images/headers/profile-covers/c5.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CN", Name = "China" }
                ),
                new(
                    5420543,
                    "PantyDev",
                    "https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.OSBWinner,
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "UA", Name = "Ukraine" }
                ),
                new(
                    12659629,
                    "Partyu",
                    "https://assets.ppy.sh/user-profile-covers/12659629/eecc235fa54deaaeacbbcfba6e63952680eb79b99549e3b0d629cb59f742d364.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    8623835,
                    "Peter",
                    "https://assets.ppy.sh/user-profile-covers/8623835/8cc15e93f5ddac65115434992c1e579a49184c1b781b4b196fd537341b3a447b.gif",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "PL", Name = "Poland" }
                ),
                new(
                    10553003,
                    "Pieseu",
                    "https://assets.ppy.sh/user-profile-covers/10553003/9d374a62a0212cd87dc5809972bf11aac2ba9df69698baddf4d47f5dddd9eebe.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "GB", Name = "United Kingdom" }
                ),
                new(
                    7149815,
                    "Plaudible",
                    "https://assets.ppy.sh/user-profile-covers/7149815/bfa904f3909e69d9f122afd7877449c79943918f0d1fa00f92c7358213f4c4b5.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    10579063,
                    "Ponilek",
                    "https://assets.ppy.sh/user-profile-covers/10579063/6ecafeed6f005953e431f2e6f30025ea428d50e6ce8c9fef1a032e09065396c9.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "PL", Name = "Poland" }
                ),
                new(
                    8422432,
                    "Rhonen",
                    "https://assets.ppy.sh/user-profile-covers/8422432/63c3f6887d067d19e8d1969751314469997a6b75decf84a98f5123ca2da97ce3.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    5379679,
                    "Rieri",
                    "https://assets.ppy.sh/user-profile-covers/5379679/566c8d374afbeafa739cb6e2db24fa1e8b2f15efc138abbbb531eb3513651acd.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "KR", Name = "South Korea" }
                ),
                new(
                    3056732,
                    "Rizen",
                    "https://assets.ppy.sh/user-profile-covers/3056732/c0ce383b9b6b4cb7b6d7b61cbf4f72e00dbab7a53e98005e124c2bcfb8011aa8.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "NZ", Name = "New Zealand" }
                ),
                new(
                    12032602,
                    "robicoco101",
                    "https://assets.ppy.sh/user-profile-covers/12032602/69c309e842ebc7efbb3b7005636ec919309131c5c890e3888a8af8a8641e0dc0.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "RO", Name = "Romania" }
                ),
                new(
                    352328,
                    "rrtyui",
                    "https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "JP", Name = "Japan" }
                ),
                new(
                    1472763,
                    "S2VX",
                    "https://assets.ppy.sh/user-profile-covers/1472763/9cc1782b0c74fd2b0b85d075a8992dcebafc4cee108e8d7521e6d0cc7ebaddf1.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    5504231,
                    "Shmiklak",
                    "https://assets.ppy.sh/user-profile-covers/5504231/ad12be91d98a6e6359c898e74bd317bca0389db656500f2ee2f0c82d66786eba.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "RU", Name = "Russian Federation" }
                ),
                new(
                    7985423,
                    "Shortthu",
                    "https://assets.ppy.sh/user-profile-covers/7985423/9880a91dcb1bf381926fcf1a74669a574604d7c27805dc90ee3a09cafd1d13d0.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "VN", Name = "Vietnam" }
                ),
                new(
                    7798305,
                    "Slyk",
                    "https://assets.ppy.sh/user-profile-covers/7798305/ee1039f715a608d5ff029cf770db659e1fc3091b0a9fd369f1ef49c2d20a1d8b.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "MY", Name = "Malaysia" }
                ),
                new(
                    5084531,
                    "smau5",
                    "https://osu.ppy.sh/images/headers/profile-covers/c2.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "BO", Name = "Bolivia" }
                ),
                new(
                    10063190,
                    "Smug Nanachi",
                    string.Empty,
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    CountryModel.Unknown
                ),
                new(
                    8847379,
                    "SneakY NickY",
                    "https://assets.ppy.sh/user-profile-covers/8847379/6c9f731a43e47736f95df2d0429f0b2949f6b29200f5e5aa9a89d5b96e6a632c.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "SK", Name = "Slovakia" }
                ),
                new(
                    2964029,
                    "Spkz",
                    "https://assets.ppy.sh/user-profile-covers/2964029/2a3ced6b918ff7cf5df906d88787744f2132e835886f6020c72ddde64bf22e5b.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "HU", Name = "Hungary" }
                ),
                new(
                    6383137,
                    "Starset",
                    "https://osu.ppy.sh/images/headers/profile-covers/c7.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "TN", Name = "Tunisia" }
                ),
                new(
                    6124671,
                    "Storyboarder",
                    "https://assets.ppy.sh/user-profile-covers/6124671/50c6cb59ce9504c1a6d18012d6537b3bae34ac2d0234fa70f2dff91edc5e5a7e.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    6160235,
                    "Sytarno",
                    "https://assets.ppy.sh/user-profile-covers/6160235/d8d629a6b09c2dc224d563893821cccc91bfab0632fbf4e2434ab00c0ad2df07.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    2732340,
                    "Taeyang",
                    "https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "KR", Name = "South Korea" }
                ),
                new(
                    4098393,
                    "Tarrasky",
                    "https://osu.ppy.sh/images/headers/profile-covers/c2.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "BR", Name = "Brazil" }
                ),
                new(
                    7405768,
                    "TheDuckMask",
                    "https://assets.ppy.sh/user-profile-covers/7405768/e4b15ac1c2ed9c94c1bd5dc5dd3de941319d67b2ed64137646c1a8b672d59d36.gif",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CA", Name = "Canada" }
                ),
                new(
                    14004374,
                    "TheSword",
                    "https://assets.ppy.sh/user-profile-covers/14004374/ee548a0e532e34207bcbf18e2fad057ccf4675fd65267848e4e2ed3a963151d5.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "IL", Name = "Israel" }
                ),
                new(
                    10974581,
                    "Tommy Phelps",
                    "https://assets.ppy.sh/user-profile-covers/10974581/01079575dc5ed4df22e8eee843094a9240dc4e1b6c1436644d8ad56b098c1987.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "RU", Name = "Russian Federation" }
                ),
                new(
                    515967,
                    "Ulysses",
                    "https://osu.ppy.sh/images/headers/profile-covers/c8.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "HK", Name = "Hong Kong" }
                ),
                new(
                    12243368,
                    "VenerableNyanta",
                    "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "DZ", Name = "Algeria" }
                ),
                new(
                    5999631,
                    "Venix",
                    "https://assets.ppy.sh/user-profile-covers/5999631/7d4aa6dc8277ef251a71a915026bb019f46d80bded2b8ccd883edbc99f7664c2.png",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "PL", Name = "Poland" }
                ),
                new(
                    4323406,
                    "VINXIS",
                    "https://assets.ppy.sh/user-profile-covers/4323406/23ae1a56f97c653703393ca0febccdc6f4e8ad588fdb919574941193b6a2a9c3.png",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CA", Name = "Canada" }
                ),
                new(
                    5773079,
                    "Walavouchey",
                    "https://osu.ppy.sh/images/headers/profile-covers/c7.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "SE", Name = "Sweden" }
                ),
                new(
                    1331447,
                    "XinCrin",
                    "https://assets.ppy.sh/user-profile-covers/1331447/b5a97987173f57507349352b01ea66a963c02427237baab73f21b3daf459515f.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "PE", Name = "Peru" }
                ),
                new(
                    1243669,
                    "yf_bmp",
                    "https://assets.ppy.sh/user-profile-covers/1243669/9655c2d17f5be6a0b8e224e8475fa6ca83a18bc0d2ace582c67fad64615cbc25.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CN", Name = "China" }
                ),
                new(
                    1806962,
                    "Yumeno Himiko",
                    "https://assets.ppy.sh/user-profile-covers/1806962/1e7d5f2cfffb85c62006d26108ec5965c5032eb9eb55b33ec79f1a6f1b91deb0.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CN", Name = "China" }
                ),
                new(
                    6644401,
                    "Yuuma",
                    "https://assets.ppy.sh/user-profile-covers/6644401/4cdd3594e5c8cf154f1e18d2c024e35c9b6322de49cf0283bb06c61e76adce81.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "JP", Name = "Japan" }
                ),
                new(
                    4260033,
                    "Zer0-",
                    "https://assets.ppy.sh/user-profile-covers/4260033/45e8303c8e37af2d513d023387671539a971484e90dc33082524033e3727734a.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "SE", Name = "Sweden" }
                ),
                new(
                    1715876,
                    "Zexous",
                    "https://osu.ppy.sh/images/headers/profile-covers/c5.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Storyboarder,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    12941916,
                    "Apis035",
                    "https://osu.ppy.sh/images/headers/profile-covers/c8.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "ID", Name = "Indonesia" }
                ),
                new(
                    8543258,
                    "BramboraSK",
                    "https://assets.ppy.sh/user-profile-covers/8543258/c695a4189e7c09292f2a3ef42067eff02c94842c2a513e365147307cfb40981e.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "SK", Name = "Slovakia" }
                ),
                new(
                    11160462,
                    "Carcinogenesis",
                    "https://assets.ppy.sh/user-profile-covers/11160462/3c2f38ad50987c1dc4c1766cbf5d47a70f2d9f56005c50e7ec366759527708c0.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    6886572,
                    "DaIibor",
                    "https://assets.ppy.sh/user-profile-covers/6886572/578f11540538ab0b374699809f33a28d514fcef38cc5c0ae798affab30ab086b.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CZ", Name = "Czechia" }
                ),
                new(
                    16065409,
                    "elvodqa",
                    "https://assets.ppy.sh/user-profile-covers/16065409/6cc0b4ae6609d105abeb3ba3d5d5af4e0c89d3459e7258b7dc7fb102e8dda232.gif",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "TR", Name = "Türkiye" }
                ),
                new(
                    4422807,
                    "Erasome",
                    "https://assets.ppy.sh/user-profile-covers/4422807/595fa50209d22d65cc1dd7910819f317d0158cb0f60f522468083fdfb437e9e3.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    5666747,
                    "freihy",
                    "https://osu.ppy.sh/images/headers/profile-covers/c7.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "MY", Name = "Malaysia" }
                ),
                new(
                    8712289,
                    "h3oCharles",
                    "https://assets.ppy.sh/user-profile-covers/8712289/8e016e420f6ca857e3d19c1413716161c6d86d3295dce30c7adb4d06c7070600.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "PL", Name = "Poland" }
                ),
                new(
                    11171976,
                    "HeTo",
                    "https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "KR", Name = "South Korea" }
                ),
                new(
                    6741014,
                    "Kizuna",
                    "https://osu.ppy.sh/images/headers/profile-covers/c4.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "BE", Name = "Belgium" }
                ),
                new(
                    12241690,
                    "Li Syaoran",
                    "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    15903174,
                    "LnR",
                    "https://assets.ppy.sh/user-profile-covers/15903174/80547b646d021aa20cd1e3504ad2591ecce6593f2638cf0f91598c27c0813d72.png",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "PH", Name = "Philippines" }
                ),
                new(
                    13689336,
                    "Nizura",
                    "https://assets.ppy.sh/user-profile-covers/13689336/59d0dfb51fbd7f48a1aa2cab6cf970b3a1bdb26d5fc3ef81f71f5b497dad02d4.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "TT", Name = "Trinidad and Tobago" }
                ),
                new(
                    14068952,
                    "oppai bn",
                    "https://assets.ppy.sh/user-profile-covers/14068952/5f717099ca71ad60a189646cbc1de265ddbb0ec55fa86fc0b01cb79c758d61a0.gif",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "KR", Name = "South Korea" }
                ),
                new(
                    14272323,
                    "Orange_",
                    "https://assets.ppy.sh/user-profile-covers/14272323/871265aa563822266773dd88290ab412918b720ef178aef731142f5be530cf11.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    3142770,
                    "PainDesu",
                    "https://assets.ppy.sh/user-profile-covers/3142770/ea8e0fac61464d02717a24604a25e14f5afbd47f5ae0e7fbad01ccb1cacf91c0.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "CZ", Name = "Czechia" }
                ),
                new(
                    8976153,
                    "quangtien2005",
                    "https://osu.ppy.sh/images/headers/profile-covers/c4.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "VN", Name = "Vietnam" }
                ),
                new(
                    6812076,
                    "Rabbitt",
                    "https://assets.ppy.sh/user-profile-covers/6812076/5d9e58415c668c58987ed9cf1db2ad62130efe6cc4b1ce7a731de222eb0ed634.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    1125647,
                    "Shikibe Mayu",
                    "https://assets.ppy.sh/user-profile-covers/1125647/95b82854c3b2f2147a356d792b32487f4f95d7d51ae3377e09cacee0a0f61710.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "PH", Name = "Philippines" }
                ),
                new(
                    12123512,
                    "Tactix",
                    "https://assets.ppy.sh/user-profile-covers/12123512/dbff0990155dfd5dbb09314844ec9dab004773afd0a35936a24b3fb4a16013bc.png",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "US", Name = "United States" }
                ),
                new(
                    9404433,
                    "TaStaR",
                    "https://assets.ppy.sh/user-profile-covers/9404433/0d8e1c35e5c92440439a0ab309de6217b68ad09ee86c43d1effa70b786e9dad9.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "KR", Name = "South Korea" }
                ),
                new(
                    15416043,
                    "TheDumbHuman",
                    "https://osu.ppy.sh/images/headers/profile-covers/c4.jpg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Apprentice,
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "RO", Name = "Romania" }
                ),
                new(
                    702598,
                    "Spectator",
                    "https://assets.ppy.sh/user-profile-covers/702598/b69698a0d3ffe84308c83b6c34d9e4089272512d57deb961a88b91146e1d698e.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "KR", Name = "South Korea" }
                ),
                new(
                    13069714,
                    "Tachibana_",
                    "https://assets.ppy.sh/user-profile-covers/13069714/ebfb11d23745b762862b4fec173b4e65b53926fb1fcf50530f93232e53d745aa.jpeg",
                    new List<DiscordRoleModel>
                    {
                        DiscordRoleModel.Verified
                    },
                    new CountryModel { Code = "RU", Name = "Russian Federation" }
                )
            };
        }

        public static List<BeatmapModel> GenerateBeatmaps()
        {
            List<BeatmapModel> beatmaps = new List<BeatmapModel>
            {
                new BeatmapModel
                (
                    beatmapsetID: 16457,
                    beatmapTitle: "SWITCHED-ON LOTUS",
                    beatmapArtist: "Susumu Hirasawa",
                    beatmapsetHost: new MapperModel(410, "Starrodkirby86"),
                    medium: "Scripting",
                    submitDate: new DateTime(2010, 12, 05),
                    showcasedDate: new DateTime(2021, 12, 18),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Starrodkirby86")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Lyrics,
                        StoryboardTag.Classic
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1054045,
                    beatmapTitle: "Alexithymia | Lupinus | Tokei no Heya to Seishin Sekai",
                    beatmapArtist: "a_hisa",
                    beatmapsetHost: new MapperModel(3250792, "ProfessionalBox"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2019, 10, 20),
                    showcasedDate: new DateTime(2021, 12, 18),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("-Tochi"),
                        GetStoryboarderByName("PoNo")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Wizardry,
                        StoryboardTag.Featured
                    },
                    videoURL: "https://www.youtube.com/embed/Pha-oDmXWm0"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 470977,
                    beatmapTitle: "world.execute(me);",
                    beatmapArtist: "Mili",
                    beatmapsetHost: new MapperModel(2559349, "Exile-"),
                    medium: "Java",
                    submitDate: new DateTime(2017, 01, 29),
                    showcasedDate: new DateTime(2021, 12, 18),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Exile-")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Narrative,
                        StoryboardTag.ThreeD,
                        StoryboardTag.Lyrics,
                        StoryboardTag.Pixels,
                        StoryboardTag.Wizardry,
                        StoryboardTag.Featured
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 173288,
                    beatmapTitle: "Kung-fu Empire",
                    beatmapArtist: "Nekomata Master+",
                    beatmapsetHost: new MapperModel(989377, "Damnae"),
                    medium: "C#",
                    submitDate: new DateTime(2015, 05, 24),
                    showcasedDate: new DateTime(2021, 12, 18),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Damnae")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Pixels,
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Wizardry,
                        StoryboardTag.Featured
                    },
                    videoURL: "https://www.youtube.com/embed/AJyUXMvn45o"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 943052,
                    beatmapTitle: "Rise (feat. IZ*ONE)",
                    beatmapArtist: "Jonas Blue",
                    beatmapsetHost: new MapperModel(6607303, "Hokichi"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2019, 03, 19),
                    showcasedDate: new DateTime(2021, 12, 18),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Hokichi"),
                        GetStoryboarderByName("TheDuckMask"),
                        new StoryboarderModel(6186628, "Affirmation", string.Empty)
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Lyrics
                    },
                    videoURL: "https://www.youtube.com/embed/ERaRZ8sS5UA"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1060207,
                    beatmapTitle: "Still Into You (Sped Up & Cut Ver.)",
                    beatmapArtist: "Paramore",
                    beatmapsetHost: new MapperModel(4230454, "Froskya"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2019, 11, 03),
                    showcasedDate: new DateTime(2021, 12, 18),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Slyk"),
                        GetStoryboarderByName("PantyDev"),
                        GetStoryboarderByName("freihy"),
                        GetStoryboarderByName("BowLL"),
                        GetStoryboarderByName("TheDuckMask")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Lyrics
                    },
                    videoURL: "https://www.youtube.com/embed/XnMVs1eJJnQ"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1388552,
                    beatmapTitle: "My Love (DestoppeD Remix)",
                    beatmapArtist: "Kuba Oms",
                    beatmapsetHost: new MapperModel(2841009, "Mirash"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2021, 03, 01),
                    showcasedDate: new DateTime(2021, 12, 30),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Tommy Phelps")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Lyrics
                    },
                    videoURL: "https://www.youtube.com/embed/e6mic6yX1i8"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1579180,
                    beatmapTitle: "DEAR MY WAKER",
                    beatmapArtist: "Yonekura Chihiro",
                    beatmapsetHost: new MapperModel(873961, "Skystar"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2021, 09, 17),
                    showcasedDate: new DateTime(2021, 12, 30),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Kuse")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.Narrative,
                        StoryboardTag.Lyrics
                    },
                    videoURL: "https://www.youtube.com/embed/ubc_ipmU_1I"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1325570,
                    beatmapTitle: "Contra (feat. Sara Skinner) (Cut Ver.)",
                    beatmapArtist: "Pixel Terror",
                    beatmapsetHost: new MapperModel(12243368, "VenerableNyanta"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2020, 12, 18),
                    showcasedDate: new DateTime(2021, 12, 30),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("VenerableNyanta")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Lyrics
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 144171,
                    beatmapTitle: "Far east nightbird (kors k Remix)",
                    beatmapArtist: "Nekomata Master",
                    beatmapsetHost: new MapperModel(270377, "jonathanlfj"),
                    medium: "SGL",
                    submitDate: new DateTime(2014, 01, 21),
                    showcasedDate: new DateTime(2021, 12, 30),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Damnae")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.ThreeD,
                        StoryboardTag.Featured,
                        StoryboardTag.Classic,
                        StoryboardTag.Wizardry
                    },
                    videoURL: "https://www.youtube.com/embed/aDnHXtahyV8"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 179323,
                    beatmapTitle: "Okaerinasai (tomatomerde Remix)",
                    beatmapArtist: "Sakamoto Maaya",
                    beatmapsetHost: new MapperModel(2155578, "Azer"),
                    medium: "C#",
                    submitDate: new DateTime(2014, 05, 28),
                    showcasedDate: new DateTime(2021, 12, 30),
                    storyboarders: new List<StoryboarderModel>
                    {
                        new StoryboarderModel(2112092, "11t", string.Empty)
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Lyrics,
                        StoryboardTag.Wizardry
                    },
                    videoURL: "https://www.youtube.com/embed/pWVzrDucBEY"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 582089,
                    beatmapTitle: "Reality Distortion",
                    beatmapArtist: "Camellia vs Akira Complex",
                    beatmapsetHost: new MapperModel(352328, "rrtyui"),
                    medium: "C#",
                    submitDate: new DateTime(2017, 03, 06),
                    showcasedDate: new DateTime(2022, 01, 01),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("rrtyui")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Narrative,
                        StoryboardTag.ThreeD,
                        StoryboardTag.Rave,
                        StoryboardTag.Pixels,
                        StoryboardTag.Wizardry
                    },
                    videoURL: "https://www.youtube.com/embed/zu_DpOZ3CUQ"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 602972,
                    beatmapTitle: "On Purpose",
                    beatmapArtist: "Sabrina Carpenter",
                    beatmapsetHost: new MapperModel(2675027, "CSLM"),
                    medium: "SGL",
                    submitDate: new DateTime(2017, 04, 22),
                    showcasedDate: new DateTime(2022, 01, 10),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("CSLM")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Narrative,
                        StoryboardTag.Featured,
                        StoryboardTag.Rave,
                        StoryboardTag.Pixels,
                        StoryboardTag.Wizardry
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 432822,
                    beatmapTitle: "Brain Power Long Version",
                    beatmapArtist: "NOMA",
                    beatmapsetHost: new MapperModel(873961, "Skystar"),
                    medium: "Java",
                    submitDate: new DateTime(2016, 03, 22),
                    showcasedDate: new DateTime(2022, 01, 10),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Exile-")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.ThreeD,
                        StoryboardTag.Pixels,
                        StoryboardTag.Wizardry
                    },
                    videoURL: "https://www.youtube.com/embed/Zcv_nNMotbE"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 654313,
                    beatmapTitle: "Dans la mer de son",
                    beatmapArtist: "Camellia",
                    beatmapsetHost: new MapperModel(4610047, "PoNo"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2017, 08, 15),
                    showcasedDate: new DateTime(2022, 01, 11),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("PoNo")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Wizardry
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 694402,
                    beatmapTitle: "Railgun Roulette (VIP)",
                    beatmapArtist: "Camellia vs Akira Complex",
                    beatmapsetHost: new MapperModel(841391, "NeilPerry"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2017, 11, 15),
                    showcasedDate: new DateTime(2022, 01, 11),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("PoNo")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 703957,
                    beatmapTitle: "Immortal Flame (feat. Anna Yvette)",
                    beatmapArtist: "Panda Eyes & Teminite",
                    beatmapsetHost: new MapperModel(4452992, "Sotarks"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2017, 12, 10),
                    showcasedDate: new DateTime(2022, 01, 11),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("PoNo")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1329045,
                    beatmapTitle: "Glacierfall (Park Remix)",
                    beatmapArtist: "sakuraburst",
                    beatmapsetHost: new MapperModel(8972308, "ScubDomino"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2020, 12, 22),
                    showcasedDate: new DateTime(2022, 01, 12),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Hokichi"),
                        GetStoryboarderByName("Ningguang")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Rave,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Wizardry,
                        StoryboardTag.FullControl,
                        StoryboardTag.Pixels
                    },
                    videoURL: "https://www.youtube.com/embed/6c-yIIBaIVo"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 701482,
                    beatmapTitle: "skyshifter vip",
                    beatmapArtist: "sakuraburst",
                    beatmapsetHost: new MapperModel(8972308, "ScubDomino"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2017, 12, 03),
                    showcasedDate: new DateTime(2022, 01, 12),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Coppertine"),
                        GetStoryboarderByName("PoNo"),
                        GetStoryboarderByName("Darky1")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Rave,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.FullControl,
                        StoryboardTag.Pixels
                    },
                    videoURL: "https://www.youtube.com/embed/rmssvCNp9rE"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 581787,
                    beatmapTitle: "Der Wald (kors k Remix)",
                    beatmapArtist: "Eleharmonica remixed by kors k",
                    beatmapsetHost: new MapperModel(3700647, "Cheesecake"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2017, 03, 05),
                    showcasedDate: new DateTime(2022, 01, 14),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Darky1")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Rave,
                        StoryboardTag.Wizardry,
                        StoryboardTag.AudioVisualization
                    },
                    videoURL: "https://www.youtube.com/embed/8xnYI3WdYPg"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 595796,
                    beatmapTitle: "forest of the spirits",
                    beatmapArtist: "sakuraburst",
                    beatmapsetHost: new MapperModel(465126, "Shiirn"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2017, 04, 06),
                    showcasedDate: new DateTime(2022, 01, 14),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Jakomo73")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.FullControl
                    },
                    videoURL: "https://www.youtube.com/embed/ezYiUFuZm-A"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 460580,
                    beatmapTitle: "bouquet",
                    beatmapArtist: "ChouCho",
                    beatmapsetHost: new MapperModel(3193504, "Kibbleru"),
                    medium: "SGL",
                    submitDate: new DateTime(2016, 05, 26),
                    showcasedDate: new DateTime(2022, 01, 17),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Sidetail")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Narrative,
                        StoryboardTag.Lyrics,
                        StoryboardTag.Particles,
                        StoryboardTag.Wizardry,
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.Pixels
                    },
                    videoURL: "https://www.youtube.com/embed/ChmWubfJ0Nw"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 747823,
                    beatmapTitle: "Oriental Blossom",
                    beatmapArtist: "Eisyo-kobu",
                    beatmapsetHost: new MapperModel(702598, "Spectator"),
                    medium: "Python",
                    submitDate: new DateTime(2018, 03, 12),
                    showcasedDate: new DateTime(2022, 01, 18),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Yumeno Himiko")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Wizardry,
                        StoryboardTag.ThreeD
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 778698,
                    beatmapTitle: "Dahil Sa'Yo",
                    beatmapArtist: "Inigo Pascual",
                    beatmapsetHost: new MapperModel(2984583, "Sakary"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2018, 05, 10),
                    showcasedDate: new DateTime(2022, 01, 18),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("arviejhay")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Pixels,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Lyrics
                    },
                    videoURL: "https://www.youtube.com/embed/j9t95U0RcNQ"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 803126,
                    beatmapTitle: "Come and Get It (Razihel Remix)",
                    beatmapArtist: "Krewella",
                    beatmapsetHost: new MapperModel(7149815, "Plaudible"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2018, 06, 24),
                    showcasedDate: new DateTime(2022, 01, 18),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Hokichi")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.ThreeD,
                        StoryboardTag.Lyrics,
                        StoryboardTag.Rave,
                        StoryboardTag.Pixels
                    },
                    videoURL: "https://www.youtube.com/embed/W5VePrILlvg"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 891335,
                    beatmapTitle: "Fallen Angel",
                    beatmapArtist: "HyuN feat. JeeE",
                    beatmapsetHost: new MapperModel(2204515, "Mao"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2018, 12, 09),
                    showcasedDate: new DateTime(2022, 01, 18),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Noffy"),
                        GetStoryboarderByName("Mao")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Narrative
                    },
                    videoURL: "https://www.youtube.com/embed/2tu84H9C2IA"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 298245,
                    beatmapTitle: "Windy Fairy",
                    beatmapArtist: "DJ TOTTO",
                    beatmapsetHost: new MapperModel(3683084, "pandorawindy"),
                    medium: "Java",
                    submitDate: new DateTime(2015, 03, 29),
                    showcasedDate: new DateTime(2022, 01, 19),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("pandorawindy")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Narrative,
                        StoryboardTag.Featured,
                        StoryboardTag.Wizardry
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 349062,
                    beatmapTitle: "Aozora Memories",
                    beatmapArtist: "yuiko & Meis Clauson",
                    beatmapsetHost: new MapperModel(3193504, "Kibbleru"),
                    medium: "Python",
                    submitDate: new DateTime(2015, 08, 25),
                    showcasedDate: new DateTime(2022, 01, 19),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Gelbana")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Narrative,
                        StoryboardTag.Lyrics,
                        StoryboardTag.Featured,
                        StoryboardTag.Pixels
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1011020,
                    beatmapTitle: "DYE/Re:flection+",
                    beatmapArtist: "AVTechNO!xTreow",
                    beatmapsetHost: new MapperModel(6607303, "Hokichi"),
                    medium: "Scripting",
                    submitDate: new DateTime(2019, 07, 29),
                    showcasedDate: new DateTime(2022, 01, 19),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("TheDuckMask")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Rave,
                        StoryboardTag.ThreeD,
                        StoryboardTag.Lyrics,
                        StoryboardTag.Featured
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 395642,
                    beatmapTitle: "Okaerinasai (tomatomerde Remix)",
                    beatmapArtist: "Sakamoto Maaya",
                    beatmapsetHost: new MapperModel(3056732, "Rizen"),
                    medium: "Design Editor",
                    submitDate: new DateTime(2015, 12, 23),
                    showcasedDate: new DateTime(2022, 01, 19),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Rizen")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.FullControl,
                        StoryboardTag.Narrative,
                        StoryboardTag.Lyrics
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 411435,
                    beatmapTitle: "Boogie",
                    beatmapArtist: "Billain",
                    beatmapsetHost: new MapperModel(4761930, "Spaghetti"),
                    medium: "Other",
                    submitDate: new DateTime(2016, 01, 31),
                    showcasedDate: new DateTime(2022, 01, 21),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Storyboarder"),
                        GetStoryboarderByName("-Tochi")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Rave,
                        StoryboardTag.Particles
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 868749,
                    beatmapTitle: "Fukashigi no Karte",
                    beatmapArtist: "Seishun Buta Yarou Cast",
                    beatmapsetHost: new MapperModel(7515767, "Half"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2018, 10, 25),
                    showcasedDate: new DateTime(2022, 01, 21),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Half")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Lyrics
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 844351,
                    beatmapTitle: "Senpai.",
                    beatmapArtist: "HoneyWorks meets TrySail",
                    beatmapsetHost: new MapperModel(2202163, "-Mo-"),
                    medium: "Python",
                    submitDate: new DateTime(2018, 09, 05),
                    showcasedDate: new DateTime(2022, 01, 21),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("-Mo-")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Lyrics
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1589017,
                    beatmapTitle: "Rererepeat",
                    beatmapArtist: "frederic",
                    beatmapsetHost: new MapperModel(3086393, "n0ah"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2021, 09, 29),
                    showcasedDate: new DateTime(2022, 01, 21),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("DaIibor"),
                        GetStoryboarderByName("BramboraSK")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Lyrics
                    },
                    videoURL: "https://www.youtube.com/embed/SbHLMX6T-1o"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 242484,
                    beatmapTitle: "werewolf howls.",
                    beatmapArtist: "Camellia",
                    beatmapsetHost: new MapperModel(4568037, "Takane6"),
                    medium: "Other",
                    submitDate: new DateTime(2014, 12, 11),
                    showcasedDate: new DateTime(2022, 01, 25),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("arviejhay"),
                        new StoryboarderModel(3027421, "Arzenvald", string.Empty)
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Rave
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 245160,
                    beatmapTitle: "Wings of Courage -Sora o Koete-",
                    beatmapArtist: "Mami Kawada",
                    beatmapsetHost: new MapperModel(1806962, "Yumeno Himiko"),
                    medium: "Other",
                    submitDate: new DateTime(2014, 12, 18),
                    showcasedDate: new DateTime(2022, 01, 25),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("yf_bmp"),
                        GetStoryboarderByName("Yumeno Himiko")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Narrative
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1400282,
                    beatmapTitle: "On & On (feat. Daniel Levi) (Cut Ver.)",
                    beatmapArtist: "Cartoon",
                    beatmapsetHost: new MapperModel(3906405, "Sylas"),
                    medium: "Other",
                    submitDate: new DateTime(2021, 03, 13),
                    showcasedDate: new DateTime(2022, 02, 03),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("-Ady"),
                        GetStoryboarderByName("Partyu")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization
                    },
                    videoURL: "https://www.youtube.com/embed/M0eeJtckkaY"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1144729,
                    beatmapTitle: "Pretender (Short Ver.)",
                    beatmapArtist: "cillia",
                    beatmapsetHost: new MapperModel(3056732, "Rizen"),
                    medium: "Scripting",
                    submitDate: new DateTime(2020, 04, 10),
                    showcasedDate: new DateTime(2022, 02, 03),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Rizen")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics,
                        StoryboardTag.Particles
                    },
                    videoURL: "https://www.youtube.com/embed/U-m3UK2T_Yw"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 591442,
                    beatmapTitle: "Nostalgic (Nightcore Mix)",
                    beatmapArtist: "S3RL feat Harri Rush",
                    beatmapsetHost: new MapperModel(515967, "nold_1702"),
                    medium: "Scripting",
                    submitDate: new DateTime(2017, 03, 27),
                    showcasedDate: new DateTime(2022, 02, 09),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Ulysses"),
                        GetStoryboarderByName("yf_bmp"),
                        GetStoryboarderByName("Irreversible")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Narrative,
                        StoryboardTag.Lyrics,
                        StoryboardTag.Rave,
                        StoryboardTag.Pixels,
                        StoryboardTag.Wizardry
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 463479,
                    beatmapTitle: "Bassdrop Freaks (Long Ver.)",
                    beatmapArtist: "Nanahira",
                    beatmapsetHost: new MapperModel(1243669, "yf_bmp"),
                    medium: "C#",
                    submitDate: new DateTime(2016, 06, 02),
                    showcasedDate: new DateTime(2022, 02, 09),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("yf_bmp")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Featured,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Wizardry,
                        StoryboardTag.Lyrics
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 685539,
                    beatmapTitle: "Tegami",
                    beatmapArtist: "Nekomata Master",
                    beatmapsetHost: new MapperModel(1225459, "Kenny"),
                    medium: "C++",
                    submitDate: new DateTime(2017, 10, 24),
                    showcasedDate: new DateTime(2022, 02, 09),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("S2VX"),
                        GetStoryboarderByName("niseboi")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.Narrative,
                        StoryboardTag.Lyrics,
                        StoryboardTag.Featured,
                        StoryboardTag.Wizardry
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 143281,
                    beatmapTitle: "Stigma",
                    beatmapArtist: "Noisia",
                    beatmapsetHost: new MapperModel(157177, "Natteke"),
                    medium: "SGL",
                    submitDate: new DateTime(2014, 01, 17),
                    showcasedDate: new DateTime(2022, 02, 12),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Damnae")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles,
                        StoryboardTag.ThreeD
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 399810,
                    beatmapTitle: "Minazoko Yuuhodou",
                    beatmapArtist: "Reol",
                    beatmapsetHost: new MapperModel(1715876, "Zexous"),
                    medium: "SGL",
                    submitDate: new DateTime(2016, 01, 02),
                    showcasedDate: new DateTime(2022, 02, 12),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Zexous")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1283879,
                    beatmapTitle: "Life Adjusts (feat. Miyoki) (Cut Ver.)",
                    beatmapArtist: "Rameses B",
                    beatmapsetHost: new MapperModel(4260033, "Zer0-"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2020, 10, 23),
                    showcasedDate: new DateTime(2022, 02, 12),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Zer0-")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.Lyrics
                    },
                    videoURL: "https://www.youtube.com/embed/4Qpyp6A-AJc"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1224414,
                    beatmapTitle: "Pekorap Tropical House Remix",
                    beatmapArtist: "REDALiCE",
                    beatmapsetHost: new MapperModel(6644401, "Yuuma"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2020, 07, 29),
                    showcasedDate: new DateTime(2022, 02, 12),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Yuuma")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 842871,
                    beatmapTitle: "My Dearest",
                    beatmapArtist: "supercell",
                    beatmapsetHost: new MapperModel(2931883, "Ascendance"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2018, 07, 29),
                    showcasedDate: new DateTime(2022, 02, 12),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Storyboarder"),
                        GetStoryboarderByName("PoNo")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.Lyrics,
                        StoryboardTag.Pixels,
                        StoryboardTag.GameplayEnhancement
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1526749,
                    beatmapTitle: "Phai Chang Em Da Yeu? feat. RedT",
                    beatmapArtist: "Juky San",
                    beatmapsetHost: new MapperModel(18319978, "Ayate"),
                    medium: "JavaScript",
                    submitDate: new DateTime(2021, 07, 22),
                    showcasedDate: new DateTime(2022, 02, 16),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Smug Nanachi")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.Lyrics
                    },
                    videoURL: "https://www.youtube.com/embed/0pxx-1dlqUQ"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1083417,
                    beatmapTitle: "White Winter Hymnal",
                    beatmapArtist: "Birdy",
                    beatmapsetHost: new MapperModel(4230454, "Froskya"),
                    medium: "Scripting",
                    submitDate: new DateTime(2019, 12, 25),
                    showcasedDate: new DateTime(2022, 02, 17),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("freihy")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.Lyrics
                    },
                    videoURL: "https://www.youtube.com/embed/8T8qS3LjzEw"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1148442,
                    beatmapTitle: "Modelista",
                    beatmapArtist: "HiTECH NINJA",
                    beatmapsetHost: new MapperModel(7288862, "-jordan-"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2020, 04, 15),
                    showcasedDate: new DateTime(2022, 02, 27),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("VenerableNyanta")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.FullControl,
                        StoryboardTag.Rave
                    },
                    videoURL: "https://www.youtube.com/embed/rSXGZjDtiUw"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 424397,
                    beatmapTitle: "Give It Up",
                    beatmapArtist: "Knife Party",
                    beatmapsetHost: new MapperModel(5318910, "sahuang"),
                    medium: "Other",
                    submitDate: new DateTime(2016, 03, 01),
                    showcasedDate: new DateTime(2022, 02, 27),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Naxess")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 311393,
                    beatmapTitle: "echoes vocal. yanaginagi",
                    beatmapArtist: "project-ALCA-",
                    beatmapsetHost: new MapperModel(2683648, "Gaia"),
                    medium: "SGL",
                    submitDate: new DateTime(2015, 03, 11),
                    showcasedDate: new DateTime(2022, 02, 27),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Kibbleru"),
                        GetStoryboarderByName("Sidetail")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.FullControl,
                        StoryboardTag.Pixels,
                        StoryboardTag.Lyrics
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1314759,
                    beatmapTitle: "Cyaegha",
                    beatmapArtist: "USAO",
                    beatmapsetHost: new MapperModel(10027577, "- AzRaeL -"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2020, 12, 03),
                    showcasedDate: new DateTime(2022, 02, 27),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("- AzRaeL -")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Particles,
                        StoryboardTag.FullControl,
                        StoryboardTag.GameplayEnhancement
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 842355,
                    beatmapTitle: "Carol of the Bells (Nightstep Mix)",
                    beatmapArtist: "BarlowGirl",
                    beatmapsetHost: new MapperModel(10974170, "Nuvolina"),
                    medium: "SGL",
                    submitDate: new DateTime(2018, 09, 01),
                    showcasedDate: new DateTime(2022, 02, 27),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("[ Kuro Usagi ]")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics,
                        StoryboardTag.Particles,
                        StoryboardTag.FullControl,
                        StoryboardTag.ThreeD
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 119911,
                    beatmapTitle: "EdelWeiss",
                    beatmapArtist: "OkameP",
                    beatmapsetHost: new MapperModel(568761, "[Mahua]"),
                    medium: "Design Editor",
                    submitDate: new DateTime(2013, 10, 01),
                    showcasedDate: new DateTime(2022, 02, 28),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("[Mahua]")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics,
                        StoryboardTag.Particles,
                        StoryboardTag.Classic
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 729559,
                    beatmapTitle: "Broken",
                    beatmapArtist: "Magnitude",
                    beatmapsetHost: new MapperModel(2907231, "Asonate"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2018, 02, 03),
                    showcasedDate: new DateTime(2022, 02, 28),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Asonate")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.Particles,
                        StoryboardTag.GameplayEnhancement
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1108256,
                    beatmapTitle: "Yuune Zekka, Ryouran no Sai",
                    beatmapArtist: "EastNewSound feat. Chata & nayuta",
                    beatmapsetHost: new MapperModel(9131844, "AYE1337"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2020, 02, 12),
                    showcasedDate: new DateTime(2022, 02, 28),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("AYE1337")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics,
                        StoryboardTag.Particles,
                        StoryboardTag.GameplayEnhancement
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 800568,
                    beatmapTitle: "SAD!",
                    beatmapArtist: "XXXTENTACION",
                    beatmapsetHost: new MapperModel(3388410, "eiri-"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2018, 06, 19),
                    showcasedDate: new DateTime(2022, 02, 28),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Bergy")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics,
                        StoryboardTag.Pixels
                    },
                    videoURL: "https://www.youtube.com/embed/ih0ks6rq6A4"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 388428,
                    beatmapTitle: "Midnight Stroller",
                    beatmapArtist: "Reol",
                    beatmapsetHost: new MapperModel(3621552, "Chaoslitz"),
                    medium: "Other",
                    submitDate: new DateTime(2015, 12, 09),
                    showcasedDate: new DateTime(2022, 02, 28),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Chaoslitz"),
                        GetStoryboarderByName("Yumeno Himiko")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics,
                        StoryboardTag.AudioVisualization
                    }
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1591460,
                    beatmapTitle: "Kyu-kurarin",
                    beatmapArtist: "Iyowa feat. KAFU",
                    beatmapsetHost: new MapperModel(3812234, "0ugi"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2021, 10, 02),
                    showcasedDate: new DateTime(2022, 03, 12),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Himada")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics
                    },
                    videoURL: "https://www.youtube.com/embed/ItkwooefdY0"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1540229,
                    beatmapTitle: "2009",
                    beatmapArtist: "glaive",
                    beatmapsetHost: new MapperModel(13646997, "Le Mirai"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2021, 08, 05),
                    showcasedDate: new DateTime(2022, 03, 13),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("VenerableNyanta"),
                        GetStoryboarderByName("TheSword")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics,
                        StoryboardTag.Particles
                    },
                    videoURL: "https://www.youtube.com/embed/wrBp2s013Xo"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 355065,
                    beatmapTitle: "Beneath The Lights (Darren Styles Remix)",
                    beatmapArtist: "Molly",
                    beatmapsetHost: new MapperModel(1331447, "XinCrin"),
                    medium: "C#",
                    submitDate: new DateTime(2015, 09, 12),
                    showcasedDate: new DateTime(2022, 03, 14),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("XinCrin")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Rave,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.FullControl
                    },
                    videoURL: "https://www.youtube.com/embed/_BEtwuHqWAo"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 692496,
                    beatmapTitle: "My Non-Existent Friend, Alice",
                    beatmapArtist: "Boxplot",
                    beatmapsetHost: new MapperModel(2208964, "Secretpipe"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2017, 11, 10),
                    showcasedDate: new DateTime(2022, 03, 15),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Kawa")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.FullControl
                    },
                    videoURL: "https://www.youtube.com/embed/Rcq0qJgCZiY"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1485845,
                    beatmapTitle: "MAKE IT FUNKY NOW (BMS EDIT)",
                    beatmapArtist: "DOT96",
                    beatmapsetHost: new MapperModel(2330619, "Mr HeliX"),
                    medium: "Scripting",
                    submitDate: new DateTime(2021, 06, 08),
                    showcasedDate: new DateTime(2022, 03, 16),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Mr HeliX")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.Particles,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.FullControl,
                        StoryboardTag.Wizardry,
                        StoryboardTag.Featured
                    },
                    videoURL: "https://www.youtube.com/embed/YEvniuKC6VU"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1373824,
                    beatmapTitle: "Distance (KOAN Sound Remix)",
                    beatmapArtist: "Apashe feat. Geoffroy",
                    beatmapsetHost: new MapperModel(7919025, "Akeruyri"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2021, 02, 13),
                    showcasedDate: new DateTime(2022, 03, 17),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("TheSword")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.GameplayEnhancement,
                        StoryboardTag.Lyrics,
                        StoryboardTag.AudioVisualization,
                        StoryboardTag.FullControl,
                        StoryboardTag.Particles
                    },
                    videoURL: "https://www.youtube.com/embed/f87G2Qz6SBc"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 727329,
                    beatmapTitle: "AMAZING BREAK",
                    beatmapArtist: "TERRASPEX",
                    beatmapsetHost: new MapperModel(702598, "Spectator"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2018, 01, 29),
                    showcasedDate: new DateTime(2022, 03, 18),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Darky1")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics,
                        StoryboardTag.Particles
                    },
                    videoURL: "https://www.youtube.com/embed/SttYNzvWIfQ"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 1685032,
                    beatmapTitle: "i wanna slam my head against the wall",
                    beatmapArtist: "glaive",
                    beatmapsetHost: new MapperModel(702598, "Tachibana_"),
                    medium: "Storybrew",
                    submitDate: new DateTime(2022, 01, 28),
                    showcasedDate: new DateTime(2022, 03, 20),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Tachibana_")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics,
                        StoryboardTag.Particles
                    },
                    videoURL: "https://www.youtube.com/embed/PBmWGe2x0X4"
                ),
                new BeatmapModel
                (
                    beatmapsetID: 961320,
                    beatmapTitle: "All In All",
                    beatmapArtist: "Kuba Oms",
                    beatmapsetHost: new MapperModel(5999631, "Venix"),
                    medium: "Python",
                    submitDate: new DateTime(2022, 01, 28),
                    showcasedDate: new DateTime(2022, 03, 21),
                    storyboarders: new List<StoryboarderModel>
                    {
                        GetStoryboarderByName("Venix")
                    },
                    tags: new List<StoryboardTag>
                    {
                        StoryboardTag.Lyrics,
                        StoryboardTag.Particles
                    },
                    videoURL: "https://www.youtube.com/embed/jLqhUToiEXY"
                )
            };
            return beatmaps;
        }

        public static BeatmapModel GetRandomBeatmap()
        {
            List<BeatmapModel> beatmaps = GenerateBeatmaps();
            var random = new Random();
            return beatmaps.ElementAt(random.Next(beatmaps.Count));
        }

        public static BeatmapModel GenerateBeatmap(int beatmapsetID)
        {
            BeatmapModel beatmap = GenerateBeatmaps().Where(x => x.BeatmapsetID == beatmapsetID).FirstOrDefault();
            return beatmap;
        }

        public static List<BeatmapModel> GenerateBeatmapsByStoryboarder(int userID)
        {
            List<BeatmapModel> allBeatmaps = GenerateBeatmaps();
            List<BeatmapModel> beatmaps = new List<BeatmapModel>();
            for (int i = 0; i < allBeatmaps.Count; i++)
            {
                if (allBeatmaps[i].Storyboarders.Any(x => x.UserID == userID))
                {
                    beatmaps.Add(allBeatmaps[i]);
                }
            }

            return beatmaps;
        }

        public static StoryboarderModel GenerateStoryboarder(int userID)
        {
            StoryboarderModel storyboarder = GenerateStoryboarders().Where(x => x.UserID == userID).FirstOrDefault();
            return storyboarder;
        }

        public static StoryboarderModel GetStoryboarderByName(string username)
        {
            StoryboarderModel storyboarder =
                GenerateStoryboarders().Where(x => x.Username == username).FirstOrDefault();
            return storyboarder;
        }

        public static StoryboarderModel GetStoryboarderFromBeatmaps(int userID)
        {
            List<BeatmapModel> allBeatmaps = GenerateBeatmaps();
            for (int i = 0; i < allBeatmaps.Count; i++)
            {
                if (allBeatmaps[i].Storyboarders.Any(x => x.UserID == userID))
                {
                    return allBeatmaps[i].Storyboarders.Where(x => x.UserID == userID).First();
                }
            }

            return null;
        }
    }
}
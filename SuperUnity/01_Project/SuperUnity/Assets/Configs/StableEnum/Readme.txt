1.枚举和字符串相互转化
2.作用 枚举在UGUI [Serializable]时记录的时int 数值 当枚举变化时关联容易丢失




例子：


    public enum GameCollectionItemKey
    {
        None=0,
        AllAnswers,
        Answers,
        Collection1,
        Collection2,
        Collection3,
    }

    [Serializable]
    public class GameCollectionItemStableEnum : StableEnum< GameCollectionItemKey>
    {
        public static implicit operator GameCollectionItemStableEnum( GameCollectionItemKey value)
        {
            var stableEnum = new GameCollectionItemStableEnum()
            {
                _value = value,
                _proxy = value.ToString()
            };
            return stableEnum;
        }
    }
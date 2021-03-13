using System;

namespace UmaMadoManager.Core.Models
{
    public enum DefaultableBooleanLike
    {
        Default,
        True,
        False
    }

    public static class DefaultableBooleanLikeExtension
    {
        public static DefaultableBooleanLike ToDefaultableBooleanLike(this bool self)
        {
            return self ? DefaultableBooleanLike.True : DefaultableBooleanLike.False;
        }
    }
}

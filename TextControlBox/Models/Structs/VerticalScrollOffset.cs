using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace TextControlBoxNS.Models.Structs
{
    /// <summary>
    /// Describes the offset between content and vertical scroll area borders. 
    /// Two Double values describe the Top and Bottom offsets, respectively.
    /// </summary>
    public struct VerticalScrollOffset
    {
        private double _Top;

        private double _Bottom;

        /// <summary>
        /// The top offset of content
        /// </summary>
        public double Top
        {
            get
            {
                return _Top;
            }
            set
            {
                _Top = value;
            }
        }

        /// <summary>
        /// The bottom offset of content
        /// </summary>

        public double Bottom
        {
            get
            {
                return _Bottom;
            }
            set
            {
                _Bottom = value;
            }
        }

        /// <summary>
        /// Creates new VerticalScrollOffset object
        /// </summary>
        /// <param name="uniformLength">The top and bottom content offset</param>
        public VerticalScrollOffset(double uniformLength)
        {
            _Top = (_Bottom = uniformLength);
        }

        /// <summary>
        /// Creates new VerticalScrollOffset object
        /// </summary>
        /// <param name="top">The top offset of content</param>
        /// <param name="bottom">The bottom offset of content</param>
        public VerticalScrollOffset(double top, double bottom)
        {
            _Top = top;
            _Bottom = bottom;
        }


        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(CultureInfo.InvariantCulture);
        }

        internal string ToString(CultureInfo cultureInfo)
        {
            char numericListSeparator = TokenizerHelper.GetNumericListSeparator(cultureInfo);
            StringBuilder stringBuilder = new StringBuilder(64);
            stringBuilder.Append(InternalToString(_Top, cultureInfo));
            stringBuilder.Append(numericListSeparator);
            stringBuilder.Append(InternalToString(_Bottom, cultureInfo));
            return stringBuilder.ToString();
        }

        internal string InternalToString(double l, CultureInfo cultureInfo)
        {
            if (double.IsNaN(l))
            {
                return "Unset";
            }

            return Convert.ToString(l, cultureInfo);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is VerticalScrollOffset verticalScrollOffset)
            {
                return this == verticalScrollOffset;
            }

            return false;
        }
        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>true if verticalScrollOffset and this instance are represent the same value; otherwise, false.</returns>
        public readonly bool Equals(VerticalScrollOffset verticalScrollOffset)
        {
            return this == verticalScrollOffset;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _Top.GetHashCode() ^ _Bottom.GetHashCode();
        }

        /// <summary>
        /// Compares two VerticalScrollOffset objects
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns>true if offsets are same; otherwise, false.</returns>
        public static bool operator ==(VerticalScrollOffset t1, VerticalScrollOffset t2)
        {
            if (t1._Top == t2._Top)
            {
                return t1._Bottom == t2._Bottom;
            }

            return false;
        }

        /// <summary>
        /// Compares two VerticalScrollOffset objects
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns>true if offsets are different; otherwise, false.</returns>
        public static bool operator !=(VerticalScrollOffset t1, VerticalScrollOffset t2)
        {
            return !(t1 == t2);
        }
    }
}

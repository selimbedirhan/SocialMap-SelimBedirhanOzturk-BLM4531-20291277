namespace SocialMap.Business.Utils;

/// <summary>
/// Geohash encoding/decoding utility for location-based clustering.
/// Geohash is a geocoding method that encodes latitude and longitude into a single string.
/// </summary>
public static class GeohashUtil
{
    private const string Base32 = "0123456789bcdefghjkmnpqrstuvwxyz";
    private static readonly int[] Bits = { 16, 8, 4, 2, 1 };

    /// <summary>
    /// Encodes latitude and longitude into a geohash string.
    /// </summary>
    /// <param name="latitude">Latitude (-90 to 90)</param>
    /// <param name="longitude">Longitude (-180 to 180)</param>
    /// <param name="precision">Length of the geohash (1-12, default 9)</param>
    /// <returns>Geohash string</returns>
    public static string Encode(double latitude, double longitude, int precision = 9)
    {
        if (precision < 1 || precision > 12)
            precision = 9;

        var latInterval = new[] { -90.0, 90.0 };
        var lngInterval = new[] { -180.0, 180.0 };
        var geohash = new System.Text.StringBuilder();
        var bit = 0;
        var ch = 0;
        var even = true;

        while (geohash.Length < precision)
        {
            if (even)
            {
                var mid = (lngInterval[0] + lngInterval[1]) / 2;
                if (longitude > mid)
                {
                    ch |= Bits[bit];
                    lngInterval[0] = mid;
                }
                else
                {
                    lngInterval[1] = mid;
                }
            }
            else
            {
                var mid = (latInterval[0] + latInterval[1]) / 2;
                if (latitude > mid)
                {
                    ch |= Bits[bit];
                    latInterval[0] = mid;
                }
                else
                {
                    latInterval[1] = mid;
                }
            }

            even = !even;

            if (bit < 4)
            {
                bit++;
            }
            else
            {
                geohash.Append(Base32[ch]);
                bit = 0;
                ch = 0;
            }
        }

        return geohash.ToString();
    }

    /// <summary>
    /// Gets the geohash prefix length based on zoom level.
    /// Lower zoom = shorter prefix (larger area), Higher zoom = longer prefix (smaller area).
    /// </summary>
    /// <param name="zoom">Map zoom level (typically 0-18)</param>
    /// <returns>Geohash prefix length (3-9)</returns>
    public static int GetPrefixLengthForZoom(int zoom)
    {
        return zoom switch
        {
            <= 3 => 3,  // Country/continent level
            <= 5 => 4,  // Region level
            <= 7 => 5,  // State/province level
            <= 9 => 6,  // City level
            <= 11 => 7, // District level
            <= 13 => 8, // Neighborhood level
            _ => 9      // Street/building level
        };
    }

    /// <summary>
    /// Gets the bounding box for a geohash prefix.
    /// </summary>
    /// <param name="geohash">Geohash string</param>
    /// <returns>Bounding box as (minLat, maxLat, minLng, maxLng)</returns>
    public static (double minLat, double maxLat, double minLng, double maxLng) DecodeBounds(string geohash)
    {
        var latInterval = new[] { -90.0, 90.0 };
        var lngInterval = new[] { -180.0, 180.0 };
        var even = true;

        foreach (var c in geohash)
        {
            var idx = Base32.IndexOf(c);
            if (idx == -1) continue;

            for (var i = 0; i < 5; i++)
            {
                var bit = (idx & Bits[i]) != 0;
                if (even)
                {
                    var mid = (lngInterval[0] + lngInterval[1]) / 2;
                    if (bit)
                        lngInterval[0] = mid;
                    else
                        lngInterval[1] = mid;
                }
                else
                {
                    var mid = (latInterval[0] + latInterval[1]) / 2;
                    if (bit)
                        latInterval[0] = mid;
                    else
                        latInterval[1] = mid;
                }
                even = !even;
            }
        }

        return (latInterval[0], latInterval[1], lngInterval[0], lngInterval[1]);
    }
}


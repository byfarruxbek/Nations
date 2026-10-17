# Country data

Made with Natural Earth. Public domain, version 5.1.2, 1:110 million admin-0 countries.

Source: https://raw.githubusercontent.com/nvkelso/natural-earth-vector/v5.1.2/geojson/ne_110m_admin_0_countries.geojson
License: https://www.naturalearthdata.com/about/terms-of-use/
Boundary policy: https://www.naturalearthdata.com/about/disputed-boundaries-policy/

Natural Earth's de facto representation is used without inventing political boundaries. The outlines include coastlines and polygon boundaries. Depiction and names do not express endorsement of sovereignty or territorial claims. This layer does not draw separate claim lines or settle disputes. 177 geographic entities are represented; small territories absent at this scale are not selectable. Holes and multipart geometry are preserved. International-date-line rings are unwrapped for hit testing.

`Tools/prepare_countries.py SOURCE.geojson` regenerates the compact JSON. Geographic coordinates remain WGS84 longitude/latitude; at runtime they are projected through the Cesium georeference using double precision before conversion to the camera's local frame. Labels use the dataset's label points and ranking. Labels behind the ellipsoid are culled, and only 16 non-overlapping labels are displayed. Boundaries use a lower vertex density at space scale.

This is a global overview layer, not a legal or cadastral dataset. Borders and labels fade below 300 km and are hidden below 80 km to avoid suggesting city-level precision. Selection uses the source polygons at all scales, so coastal and disputed-area picks inherit the dataset's limitations. Terrain occlusion is approximated by the WGS84 ellipsoid.

# Original audio

The prepared original Nations audio pack now replaces runtime synthesis. See [AUDIO_INTEGRATION.md](AUDIO_INTEGRATION.md) and Assets/_Nations/Audio/README.txt for provenance, import settings and verification.

#!/usr/bin/env python3
"""Convert the pinned, public-domain Natural Earth GeoJSON to Unity's compact schema.
Download source separately; pass the file path. No credentials or paid services required.
"""
import hashlib
import json
import sys
from pathlib import Path
source = Path(sys.argv[1])
data = json.loads(source.read_text())
countries = []
for feature in data['features']:
    p, geometry = feature['properties'], feature['geometry']
    polygons = geometry['coordinates'] if geometry['type'] == 'MultiPolygon' else [geometry['coordinates']]
    countries.append(dict(id=p['ISO_A2_EH'] if p['ISO_A2_EH']!='-99' else 'NE-'+p['ADM0_A3'], name=p['NAME'], rank=p['LABELRANK'], lon=p['LABEL_X'], lat=p['LABEL_Y'],
        polygons=[dict(rings=[dict(points=[v for xy in ring for v in xy[:2]]) for ring in polygon]) for polygon in polygons]))
output = Path(__file__).resolve().parents[1] / 'Assets/_Nations/Resources/NationsCountries.json'
output.write_text(json.dumps(dict(countries=sorted(countries, key=lambda c:c['rank'])),separators=(',',':')))
print(f'{len(countries)} geographic entities. Source SHA256: {hashlib.sha256(source.read_bytes()).hexdigest()}')

#!/usr/bin/env python3
"""Offline preparation. Inputs: Natural Earth, mledoze/countries and CLDR 48 XML in /tmp.
Flag conversion uses CairoSVG; runtime never downloads these assets. See DATA_LOCALIZATION.md.
"""
from pathlib import Path
import json, xml.etree.ElementTree as ET, unicodedata, hashlib, shutil
root=Path(__file__).resolve().parents[1]
resources=root/'Assets/_Nations/Resources'
overrides=json.loads((root/'Tools/localization_overrides.json').read_text())
entries={}
def add(key,en,uz,ru):entries[key]=dict(key=key,en=en,uz=uz,ru=ru)
def norm(s):return ''.join(c for c in unicodedata.normalize('NFKD',s) if not unicodedata.combining(c)).lower().replace('-',' ').replace('’',"'")
territories={};cities={}
for lang in ['en','uz','ru']:
    tree=ET.parse('/tmp/nations-'+lang+'.xml')
    territories[lang]={e.attrib['type']:e.text for e in tree.findall('.//localeDisplayNames/territories/territory') if 'alt' not in e.attrib}
    cities[lang]={norm(e.attrib['type'].split('/')[-1].replace('_',' ')):e.findtext('exemplarCity') for e in tree.findall('.//timeZoneNames/zone') if e.find('exemplarCity') is not None}
ui=[
('menu','Menu','Menyu','Меню'),
('audio','Audio','Ovoz','Звук'),
('music','Music','Musiqa','Музыка'),('sfx','Sound effects','Ovoz effektlari','Звуковые эффекты'),
('layers','Map layers','Xarita qatlamlari','Слои карты'),('borders','Country borders','Davlat chegaralari','Границы стран'),
('labels','Country names','Davlat nomlari','Названия стран'),('on','On','Yoqilgan','Вкл.'),('off','Off','O‘chirilgan','Выкл.'),
('language','Language','Til','Язык'),
('capital','Capital','Poytaxt','Столица'),('capitals','Capitals / seats of government','Poytaxtlar / hukumat qarorgohlari','Столицы / резиденции правительства'),
('continent','Continent','Qit’a','Континент'),('close','Close','Yopish','Закрыть'),
('data.unavailable','Data unavailable','Ma’lumot mavjud emas','Данные отсутствуют'),
('flag.unavailable','Flag unavailable','Bayroq mavjud emas','Флаг отсутствует'),
('note.disputed','Disputed territory; source representation','Bahsli hudud; manbadagi tasvir','Спорная территория; данные источника'),
('note.caucasus','Continent grouping follows the Caucasus convention','Qit’alar Kavkaz talqiniga ko‘ra berilgan','Континенты указаны по кавказской границе'),
('status.starting','Starting globe…','Globus ishga tushmoqda…','Запуск глобуса…'),
('status.streaming','Streaming Earth…','Yer xaritasi yuklanmoqda…','Загрузка Земли…'),
('status.setup','Setup globe · add a map key to stream Earth','Globusni sozlash · xarita kalitini kiriting','Настройка глобуса · добавьте ключ карты'),
('status.initialization','Map initialization failed · check local configuration','Xarita ishga tushmadi · mahalliy sozlamalarni tekshiring','Не удалось запустить карту · проверьте настройки'),
('status.unavailable','Map unavailable · check network, API access and billing','Xarita mavjud emas · internet, API va to‘lov sozlamalarini tekshiring','Карта недоступна · проверьте сеть, доступ к API и оплату'),
('status.offline','Offline · reconnect to stream Earth','Internet yo‘q · xaritani yuklash uchun ulaning','Нет сети · подключитесь для загрузки Земли'),
('status.reconnecting','Reconnecting to Earth…','Xaritaga qayta ulanmoqda…','Повторное подключение к Земле…'),
('status.exhausted','Earth unavailable · check connection and map access','Yer xaritasi mavjud emas · ulanish va ruxsatni tekshiring','Земля недоступна · проверьте подключение и доступ'),
('status.waiting','Still connecting · check network and map configuration','Ulanish davom etmoqda · internet va xarita sozlamalarini tekshiring','Подключение… Проверьте сеть и настройки карты')]
for row in ui:
    if row[0] not in ("title","resume","scroll"):add(*row)
for key,en,uz,ru in [('administrative','administrative','ma’muriy','административная'),('legislative','legislative','qonunchilik','законодательная'),('judicial','judicial','sud','судебная'),('constitutional','constitutional','konstitutsiyaviy','конституционная'),('government','government seat','hukumat qarorgohi','резиденция правительства'),('royalLegislative','royal / legislative','qirollik / qonunchilik','королевская / законодательная'),('claimed','claimed','da’vo qilingan','заявленная')]:add('capital.role.'+key,en,uz,ru)
for en,uz,ru in [('Asia','Osiyo','Азия'),('Europe','Yevropa','Европа'),('Africa','Afrika','Африка'),('North America','Shimoliy Amerika','Северная Америка'),('South America','Janubiy Amerika','Южная Америка'),('Oceania','Okeaniya','Океания'),('Antarctica','Antarktida','Антарктида')]:add('continent.'+en,en,uz,ru)
raw=json.load(open('/tmp/nations-countries.geojson'));source={c['cca2']:c for c in json.load(open('/tmp/nations-metadata.json'))}
geo=[];metadata=[];fallbacks=[]
flags=next(Path('/tmp/nations-flags-extract').glob('flag-icons-*'))/'flags/4x3'
(resources/'Flags').mkdir(exist_ok=True)
import cairosvg
for feature in raw['features']:
    p=feature['properties'];code=p['ISO_A2_EH'];code=code if code!='-99' else 'NE-'+p['ADM0_A3'];country=source.get(code)
    g=feature['geometry'];polygons=g['coordinates'] if g['type']=='MultiPolygon' else [g['coordinates']]
    geo.append(dict(id=code,name=p['NAME'],rank=p['LABELRANK'],lon=p['LABEL_X'],lat=p['LABEL_Y'],polygons=[dict(rings=[dict(points=[v for xy in ring for v in xy[:2]]) for ring in polygon]) for polygon in polygons]))
    names={lang:territories[lang].get(code,p['NAME']) for lang in territories}
    for lang in names:
        if names[lang]=='↑↑↑':names[lang]=names['en']
    if code=='NE-CYN':names=dict(en='Northern Cyprus',uz='Shimoliy Kipr',ru='Северный Кипр')
    if code=='NE-SOL':names=dict(en='Somaliland',uz='Somalilend',ru='Сомалиленд')
    add('country.'+code,names['en'],names['uz'],names['ru'])
    capitals=overrides['capitals'].get(code,[(c,'') for c in country['capital']] if country else [])
    caps=[]
    for cap,role in capitals:
        key='capital.'+norm(cap).replace(' ','_');translations={}
        for lang in ['uz','ru']:
            value=cities[lang].get(norm(cap))
            if value=='↑↑↑':value=cap
            if cap in overrides['capitalTranslations']:value=overrides['capitalTranslations'][cap][0 if lang=='uz' else 1]
            if not value:fallbacks.append(f'{code}: {cap} ({lang})');value=cap
            translations[lang]=value
        add(key,cap,translations['uz'],translations['ru']);caps.append(dict(key=key,role=role))
    flag=flags/(code.lower()+'.svg');flagKey=''
    if flag.exists() and code!='AQ':
        flagKey='Flags/'+code.lower();cairosvg.svg2png(url=str(flag),write_to=str(resources/(flagKey+'.png')),output_width=192,output_height=144)
    note='note.disputed' if code in ['NE-CYN','NE-SOL','XK','PS','EH'] else 'note.caucasus' if code in ['AZ','GE'] else ''
    metadata.append(dict(id=code,nameKey='country.'+code,flag=flagKey,capitals=caps,continents=overrides['continents'].get(code,[p['CONTINENT']]),noteKey=note))
(resources/'NationsCountries.json').write_text(json.dumps(dict(countries=sorted(geo,key=lambda c:c['rank'])),separators=(',',':'),ensure_ascii=False))
(resources/'NationsCountryMetadata.json').write_text(json.dumps(dict(countries=sorted(metadata,key=lambda c:c['id'])),indent=2,ensure_ascii=False)+'\n')
(resources/'NationsLocalization.json').write_text(json.dumps(dict(entries=list(entries.values())),indent=2,ensure_ascii=False)+'\n')
(root/'Documentation/localization-data-audit.txt').write_text(f'Entities: {len(metadata)}\nCatalog keys: {len(entries)}\nMissing capital translations using English fallback:\n'+('\n'.join(fallbacks) or 'None')+'\nNo capital metadata: '+', '.join(c['id'] for c in metadata if not c['capitals'])+'\nNo flag artwork: '+', '.join(c['id'] for c in metadata if not c['flag'])+'\n')
print((root/'Documentation/localization-data-audit.txt').read_text())

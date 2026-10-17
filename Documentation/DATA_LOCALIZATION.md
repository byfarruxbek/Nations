# Localization and country details

The interface is built by `Assets/_Nations/Scripts/UI/ExplorationUI.cs` in the existing Globe scene. The menu has a fixed 44-unit × close target and a vertically scrolling settings viewport. Language, audio and map-layer controls remain; the introductory heading, helper and large resume button were removed in every language. The modal input lock is released on close. Native Cesium credits remain outside the menu area.

`NationsLocalization.json` contains stable keys and English/Uzbek Latin/Russian values. First launch defaults to Uzbek; `Nations.Language` PlayerPrefs stores subsequent choices. Missing values fall back to English. `NATIONS`, `byFarrukhbek` and provider attribution are unchanged. Noto Sans is bundled for Cyrillic and Uzbek glyph coverage.

Country IDs join Natural Earth geometry, `NationsCountryMetadata.json`, translations and local PNG flags. The existing selection card updates immediately on a language change. South Africa includes Pretoria (administrative), Cape Town (legislative) and Bloemfontein (judicial). Other explicit multiple-capital/seat records and transcontinental groupings are maintained in `Tools/localization_overrides.json`.

## Sources and licenses

- Boundaries: Natural Earth v5.1.2, 110m admin-0, public domain. See [COUNTRY_DATA.md](COUNTRY_DATA.md). This is an overview dataset with 177 entities and de facto boundary representation, not an authoritative settlement of disputes.
- Country metadata: [mledoze/countries](https://github.com/mledoze/countries), downloaded source snapshot, Open Database License 1.0. The derived metadata database is distributed under ODbL; its editable JSON and preparation sources are included. License: `Licenses/Countries-ODbL.txt`.
- Country and available city names: [Unicode CLDR release 48](https://github.com/unicode-org/cldr/tree/release-48), Unicode license in `Licenses/Unicode-CLDR.txt`. Additional UI and city translations are authored in the preparation script and override JSON. These should receive native-language editorial review before release.
- Flags: [flag-icons v7.5.0](https://github.com/lipis/flag-icons/tree/v7.5.0), MIT, copyright notice in `Licenses/FlagIcons-MIT.txt`. 174 local PNGs converted from the upstream 4:3 SVG assets; no emoji dependency.
- Font: [Noto Sans](https://github.com/notofonts/noto-fonts), SIL Open Font License, `Licenses/NotoSans-OFL.txt`.
- South Africa capital roles: [South African government](https://www.gov.za/about-sa/south-africas-provinces). Sri Lanka administrative capital: [Ministry of Foreign Affairs](https://www.mfa.gov.lk/national-profile-and-geography).

## Data limitations

Antarctica, Northern Cyprus and Somaliland have no bundled capital/flag records in the selected metadata source; the UI explicitly displays unavailable data instead of inventing it. Small countries absent from the 110m boundary geometry cannot be selected. Country grouping follows source geometry; continent conventions are inherently variable around the Caucasus. Disputed records carry a neutral source note. All catalog entries contain three values, but proper names may legitimately match English and authored translations remain subject to editorial review.

## Regeneration

Runtime needs no network requests for metadata, flags or translations. `Tools/prepare_localization.py` expects downloaded inputs at `/tmp/nations-countries.geojson`, `/tmp/nations-metadata.json`, `/tmp/nations-{en,uz,ru}.xml`, and flag SVGs under `/tmp/nations-flags-extract/flag-icons-*/flags/4x3`. Use Python with CairoSVG and Cairo installed to regenerate resources. Source URLs are pinned above where releases exist; snapshot hashes below identify unversioned downloads. `Tools/prepare_countries.py SOURCE.geojson` regenerates geometry only and preserves stable IDs.

## Editor verification

See `localization-checks.txt`, `experience-checks.txt`, `experience-navigation.txt` and `localization-persistence.txt`. Tests cover all three languages and UZ/GB/RU/JP/BR/AU/ZA flags, capital records and continents; glyph coverage; × close/unlock; setting storage; geographic selection; portrait 1080×1920; simulated safe-area containment; streaming and credit binding. Native editor clicks also verified the simplified menu and music toggle. Physical Android touch, device audio output and device performance remain untested. ScrollRect remains enabled for constrained viewports.

Existing Cesium coarse-triangle and editor account-service warnings may remain. Script reload can cancel an editor ion connection; this does not replace or change the working project token.

## Source snapshot SHA-256

- `countries.geojson`: `6866c877d39cba9c357620878839b336d569f8c662d3cfab4cb1dbe2d39c977f`
- `metadata.json`: `913e5d716f9dc6b59881ee23488d47f5fda52070d1a34cac5fa327c9e61d8f7c`
- `en.xml`: `416fb34c500e969a9e3e9bd1e363f8af5938df3cdda72aa7e636fa7e72123aae`
- `uz.xml`: `3de666d980e8ce44381b44fc0ce55d33035c14ba40dcbfa31aae22b412d18cd1`
- `ru.xml`: `c3c8a1c92689b2d9f9c1357e0f7329e2a3bcd105bbf92049fbe7dc5c2334d253`
- `flags.tar.gz`: `5502d1bb0bda9f258d726d3c084a2d57a07cfdfa6d2ed18cbb5a1ee11b307778`

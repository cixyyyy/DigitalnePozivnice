import re,glob,json,sys,html
D="/home/claude/Korak10/tools/data/"
CODES=["sr","hr","bs","sl","de","en"]
ekav=[("Svjež","Svež"),("svjež","svež"),("Zvjezd","Zvezd"),("zvjezd","zvezd"),("mjestu","mestu"),("pomjer","pomer"),("premješt","premešt"),("smješt","smešt"),("uspjel","uspel"),("Nježn","Nežn"),("nježn","nežn"),("Mjesec","Mesec"),("vrijeme","vreme"),("Vrijeme","Vreme"),("mjesto","mesto"),("Mjesto","Mesto"),("mjesta","mesta"),("mjesec","mesec"),("dijeljenje","deljenje"),("djeljenje","deljenje"),
("vjenčanj","venčanj"),("Vjenčanj","Venčanj"),("odijeljen","odeljen"),("podijel","podel"),("Podijel","Podel"),("Cijel","Cel"),("cijel","cel"),("Cijen","Cen"),("cijen","cen"),
("uvijek","uvek"),("lijep","lep"),("Lijep","Lep"),("svjetl","svetl"),("Svjetl","Svetl"),("svijet","svet"),("osvježava","osvežava"),("osvježi","osveži"),("zauvijek","zauvek"),
("sjedenj","sedenj"),("Sjedenj","Sedenj"),("Poslije","Posle"),("poslije","posle"),("ovdje","ovde"),("Ovdje","Ovde"),("gdje","gde"),("Gdje","Gde"),("cvjetn","cvetn"),("Cvjetn","Cvetn"),
("Cvjetni","Cvetni"),("zavjes","zavesu".replace("u","")),("Zavjes","Zaves"),("mijenj","menj"),("Mijenj","Menj"),("ljepš","lepš"),("najljepš","najlepš"),("smijeh","smeh"),
("izmjen","izmen"),("Izmjen","Izmen"),("promijen","promen"),("Promijen","Promen"),("dijet","det"),("djet","det"),("Djet","Det"),("rođendan","rođendan"),("pjes","pes"),
("Pijesak","Pesak"),("mlijek","mlek"),("naslijed","nasled"),("razumij","razumev"),("htjel","hteli"),("vidjet","videt"),("vidjeti","videti"),("vidjel","videl"),("Vidjel","Videl"),
("bijel","bel"),("Bijel","Bel"),("snijeg","sneg"),("dosljed","dosled"),("rijec","reč"),("pregled","pregled"),("ostavljen","ostavljen"),("kasnij","kasnij"),("srijeda","sreda"),("nedjelja","nedelja"),("ponedjeljak","ponedeljak"),
("Zahvalni","Zahvalni"),("njemački","nemački"),("njemačkom","nemačkom"),("slovenskom","slovenačkom"),("Podrazumijevana","Podrazumevana"),("opciono","opciono"),("pratilj","pratilj"),("dobij","dobij"),("trenutno","trenutno"),
("mjehur","mehur"),("Mjehur","Mehur"),("primjer","primer"),("Primjer","Primer"),("svijetl","svetl"),("Svijetl","Svetl"),("dijel","del"),("Dijel","Del"),("mjerenj","merenj"),("Dugme","Dugme"),("sjeć","sećan"),("Sjeć","Sećan"),("sjetit","setit"),("Tijek","Tok"),("vrijedn","vredn"),("Izbor","Izbor"),("žalj","žalj"),("Pjesm","Pesm"),("pjesm","pesm"),("ljubav","ljubav"),("blagoslov","blagoslov"),("djeteta","deteta"),("djetet","detet"),("Djeteta","Deteta"),("djece","dece"),("dijete","dete")]
def ekavian(s):
    for a,b in ekav: s=s.replace(a,b)
    s=re.sub(r"\bdio\b","deo",s); s=re.sub(r"\bDio\b","Deo",s)
    return s
hr_map=[("Registruj","Registriraj"),("registruj","registriraj"),("Ko ","Tko "),(" ko "," tko "),("Personalizuj","Personaliziraj"),("Koverta","Omotnica"),("koverta","omotnica"),("kovertu","omotnicu"),("kovertama","omotnicama"),("Odabir","Odabir"),("sjedenja","sjedenja"),("opciono","neobavezno"),("Opciono","Neobavezno"),("zavesu","zavjesu"),("Zavesa","Zavjesa"),("zavesa","zavjesa"),("Pozivnica poslata","Pozivnica poslana"),("katalog","katalog"),("sa izabranim","s odabranim"),("Izaberite","Odaberite"),("izaberite","odaberite"),("Izaberi","Odaberi"),("izabran","odabran"),("Izabran","Odabran"),("Sačuvaj","Spremi"),("Sačuvajte","Spremite"),("Sačuvano","Spremljeno"),("sačuvan","spremljen"),("sačuvaj","spremi"),("Pretraži","Pretraži"),("lista ","popis "),("listu ","popis "),("liste ","popisa "),("Lista ","Popis "),("tabele","tablice"),("spisak","popis"),("štampu","tisak"),("štampa","tisak"),("Preuzmi","Preuzmi"),("Mjesec","Mjesec"),("dana ","dana "),("ručicu","ručicu"),("Mature","Mature"),("rođendane","rođendane"),("Valuta","Valuta"),("Ukupno","Ukupno"),("muziku","glazbu"),("Muziku","Glazbu"),("muzika","glazba"),("muzike","glazbe"),("Uključi muziku","Uključi glazbu"),("poruka","poruka"),("sistem","sustav"),("tabla","ploča"),("Tabla","Ploča"),("odmah","odmah"),("jedinstven","jedinstven"),("Jedinstven","Jedinstven"),("izmišljen","izmišljen"),("hoćete","hoćete"),("kreira","kreira"),("sale","dvorane"),("salu","dvoranu"),("sali","dvorani"),("Zumiranje sale","Zumiranje dvorane"),("Prikaz sale","Prikaz dvorane")]
def hr_conv(s):
    for a,b in hr_map: s=s.replace(a,b)
    return s
def parse():
    ent={}
    order=[]
    for p in sorted(glob.glob(D+"*.txt")):
        for ln in open(p,encoding='utf-8'):
            ln=ln.rstrip('\n')
            if not ln.strip(): continue
            parts=[x.strip() for x in ln.split(' ## ')]
            if len(parts)<4: print("BAD",ln[:80]); continue
            src=re.sub(r'\s+',' ',parts[0])
            d={"en":parts[1],"de":parts[2],"sl":parts[3]}
            for x in parts[4:]:
                m=re.match(r'(sr|hr|bs)=(.*)$',x)
                if m: d[m.group(1)]=m.group(2)
            if src not in ent: order.append(src)
            ent[src]=d
    return ent,order
if __name__=="__main__":
    ent,order=parse()
    print(len(ent))

# ------------- generacija C# -------------
RULES=[
 (r"^Pogledaj uživo: (.+)$", ["Pogledaj uživo: {1}","Pogledaj uživo: {1}","Pogledaj uživo: {1}","Oglej si v živo: {1}","Live ansehen: {1}","Watch live: {1}"]),
 (r"^Pregled: (.+)$", ["Pregled: {1}","Pregled: {1}","Pregled: {1}","Predogled: {1}","Vorschau: {1}","Preview: {1}"]),
 (r"^Otvori pozivnicu: (.+)$", ["Otvori pozivnicu: {1}","Otvori pozivnicu: {1}","Otvori pozivnicu: {1}","Odpri vabilo: {1}","Einladung öffnen: {1}","Open invitation: {1}"]),
 (r"^Sačuvajte datum: (.+)$", ["Sačuvajte datum: {1}","Sačuvajte datum: {1}","Sačuvajte datum: {1}","Shranite datum: {1}","Save the date: {1}","Save the date: {1}"]),
 (r"^(\d+) dolazi, (\d+) ne dolazi$", ["{1} dolazi, {2} ne dolazi","{1} dolazi, {2} ne dolazi","{1} dolazi, {2} ne dolazi","{1} prihaja, {2} ne prihaja","{1} kommen, {2} kommen nicht","{1} attending, {2} not attending"]),
 (r"^Primjer dizajna – paket (.+) – podaci su izmišljeni$", ["Primer dizajna – paket {1} – podaci su izmišljeni","Primjer dizajna – paket {1} – podaci su izmišljeni","Primjer dizajna – paket {1} – podaci su izmišljeni","Primer zasnove – paket {1} – podatki so izmišljeni","Designbeispiel – Paket {1} – die Daten sind erfunden","Design example – {1} package – the data is made up"]),
 (r"^Pogledaj demo: (.+)$", ["Pogledaj demo: {1}","Pogledaj demo: {1}","Pogledaj demo: {1}","Poglej demo: {1}","Demo ansehen: {1}","Watch demo: {1}"]),
 (r"^Stavka programa (\d+): unesite naziv\.$", ["Stavka programa {1}: unesite naziv.","Stavka programa {1}: unesite naziv.","Stavka programa {1}: unesite naziv.","Postavka programa {1}: vnesite naziv.","Programmpunkt {1}: Bitte Bezeichnung eingeben.","Programme item {1}: please enter a name."]),
 (r"^(\d+) gostiju$", ["{1} gostiju","{1} gostiju","{1} gostiju","{1} gostov","{1} Gäste","{1} guests"]),
 (r"^(\d+) gosta$", ["{1} gosta","{1} gosta","{1} gosta","{1} gosta","{1} Gäste","{1} guests"]),
 (r"^(\d+)% dolazi$", ["{1}% dolazi","{1}% dolazi","{1}% dolazi","{1}% prihaja","{1}% kommen","{1}% attending"]),
 (r"^Dolaze \((\d+)\)$", ["Dolaze ({1})","Dolaze ({1})","Dolaze ({1})","Prihajajo ({1})","Kommen ({1})","Attending ({1})"]),
 (r"^Ne dolaze \((\d+)\)$", ["Ne dolaze ({1})","Ne dolaze ({1})","Ne dolaze ({1})","Ne prihajajo ({1})","Kommen nicht ({1})","Not attending ({1})"]),
 (r"^uključujući (\d+) u pratnji$", ["uključujući {1} u pratnji","uključujući {1} u pratnji","uključujući {1} u pratnji","vključno z {1} v spremstvu","einschließlich {1} Begleitpersonen","including {1} companions"]),
 (r"^(.+) · paket (.+)$", ["{1} · paket {2}","{1} · paket {2}","{1} · paket {2}","{1} · paket {2}","{1} · Paket {2}","{1} · {2} package"]),
 (r"^PAKET (.+)$", ["PAKET {1}","PAKET {1}","PAKET {1}","PAKET {1}","PAKET {1}","{1} PACKAGE"]),
 (r"^Dolazite\.$", ["Dolazite.","Dolazite.","Dolazite.","Prihajate.","Sie kommen.","You are coming."]),
 (r"^Dolazite \(dodatnih osoba: (\d+)\)\.$", ["Dolazite (dodatnih osoba: {1}).","Dolazite (dodatnih osoba: {1}).","Dolazite (dodatnih osoba: {1}).","Prihajate (dodatnih oseb: {1}).","Sie kommen (zusätzliche Personen: {1}).","You are coming (additional guests: {1})."]),
 (r"^Ukupno vas dolazi: (\d+)\.$", ["Ukupno vas dolazi: {1}.","Ukupno vas dolazi: {1}.","Ukupno vas dolazi: {1}.","Skupaj prihaja: {1}.","Insgesamt kommen: {1}.","Total attending: {1}."]),
 (r"^(\d+) osobe$", ["{1} osobe","{1} osobe","{1} osobe","{1} oseb","{1} Personen","{1} people"]),
 (r"^(\d+) osoba$", ["{1} osoba","{1} osoba","{1} osoba","{1} oseb","{1} Personen","{1} people"]),
 (r"^(\d+) potvrđenih$", ["{1} potvrđenih","{1} potvrđenih","{1} potvrđenih","{1} potrjenih","{1} Zusagen","{1} confirmed"]),
 (r"^od (\d+) gostiju$", ["od {1} gostiju","od {1} gostiju","od {1} gostiju","od {1} gostov","von {1} Gästen","out of {1} guests"]),
]
JS_KEYS=["Uključi zvuk","Isključi zvuk","Zavrti ponovo ♥","Zatvori kovertu","Otvori kovertu","Dodirnite ponovo da zatvorite","Dodirnite pečat da otvorite kovertu","Zatvori zavesu","Otvori zavesu","Veliki dan je stigao ♥","Uključi muziku","Isključi muziku","Čuvanje…","Sačuvaj pozivnicu →","Sačuvaj izmjene →","Link je kopiran.","Kopirajte ručno (Ctrl+C).","Kopirano ✓","Predlog","Odbrojavanje"]
MONTHS_ABBR={"sr":["jan","feb","mar","apr","maj","jun","jul","avg","sep","okt","nov","dec"],
 "hr":["sij","vel","ozu","tra","svi","lip","srp","kol","ruj","lis","stu","pro"],
 "bs":["jan","feb","mar","apr","maj","jun","jul","aug","sep","okt","nov","dec"],
 "sl":["jan","feb","mar","apr","maj","jun","jul","avg","sep","okt","nov","dec"],
 "de":["jan","feb","mar","apr","mai","jun","jul","aug","sep","okt","nov","dez"],
 "en":["jan","feb","mar","apr","may","jun","jul","aug","sep","oct","nov","dec"]}
def _extra_js():
    ks=[]
    for ln in open(D+"g_raspored.txt",encoding="utf-8"):
        if ln.strip(): ks.append(re.sub(r'\s+',' ',ln.split(' ## ')[0].strip()))
    return ks
def _extra_js2():
    ks=[]
    import os
    f=D+"p_k21_js.txt"
    if os.path.exists(f):
        for ln in open(f,encoding="utf-8"):
            if ln.strip(): ks.append(re.sub(r'\s+',' ',ln.split(' ## ')[0].strip()))
    return ks
JS_KEYS=JS_KEYS+_extra_js()+_extra_js2()
def cs(s): return '@"'+s.replace('"','""')+'"'
def row(src,d):
    vals={}
    vals["sr"]=d.get("sr") or ekavian(src)
    vals["bs"]=d.get("bs") or src
    vals["hr"]=d.get("hr") or hr_conv(src)
    vals["sl"]=d["sl"]; vals["de"]=d["de"]; vals["en"]=d["en"]
    return [vals[c] for c in CODES]
def build():
    ent,order=parse()
    out=["// <auto-generated> Generisano iz tools/data/*.txt (python3 tools/gen.py). Ne uređivati ručno. </auto-generated>",
         "namespace DigitalnePozivnice.Localization;","",
         "internal static class Strings","{",
         "    // Redoslijed jezika u nizovima: sr, hr, bs, sl, de, en",
         "    public static readonly Dictionary<string, string[]> Map = new(StringComparer.Ordinal)","    {"]
    for src in order:
        vs=row(src,ent[src])
        out.append("        ["+cs(src)+"] = new[] { "+", ".join(cs(v) for v in vs)+" },")
    out+=["    };","","    public static readonly (string Pattern, string[] Templates)[] Rules =","    {"]
    for p,t in RULES: out.append("        ("+cs(p)+", new[] { "+", ".join(cs(x) for x in t)+" }),")
    out+=["    };","","    public static readonly string[] JsKeys =","    {"]
    for k in JS_KEYS: out.append("        "+cs(k)+",")
    out+=["    };","","    public static readonly Dictionary<string, string[]> MonthsAbbr = new()","    {"]
    for c in CODES: out.append('        ["%s"] = new[] { %s },'%(c,", ".join(cs(m) for m in MONTHS_ABBR[c])))
    out+=["    };","}",""]
    return "\n".join(out)
if __name__=="__main__" and len(sys.argv)>1:
    import os
    os.makedirs(sys.argv[1],exist_ok=True)
    open(sys.argv[1]+"/Strings.cs","w",encoding="utf-8-sig",newline="\r\n").write(build())
    print("ok")

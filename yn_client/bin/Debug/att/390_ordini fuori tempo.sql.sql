-- ordini fornitore non ancora arrivati
select d.*, dett.COD_DIPA, dett.DES_ART, dett.QTA, dett.DATA_CONS
 from FORN_DOCS d
 join FORN_DETT dett on dett.ID_FORN_DOC = d.ID_FORN_DOC
 where d.DOCUM = 'N-FOR-ORDINE'
  and not exists (select top 1 1 from FORN_RIFS r where r.NUMREG = d.NUMREG)
 order by d.DATADOC desc


 PRODOTTIFORNITORI

 -- carico i prodotti
 --  ;BLUEME005R;BLUEME023R;BLUEME051R;BLUEME052R;BLUEME009R;BLUEME011R;BLUEME054R;BLUEME026R;BLUEME055R;BLUEME056R;BLUEME014R;BLUEME057R;BLUEME012R;BLUEME028R;BLUEME053R;BLUEME045R;BLUEME024R;BLUEME025R;BLUEME013R;BLUEME044R;BLUEME027R;BLUEME002R;BLUEME059R;BLUEME008R;BLUEME046R;BLUEME047R;BLUEME048R;BLUEME049R;BLUEME017R;
 SELECT D.CodiceDiPa,Pr.QtaMinCons, pr.InfoAcquisto, (select Fornitore from VW_Fornitori f where f.IDFornitore = Pr.IDFornitoreDefault) as FornitoreDefault, D.Disponibilita
  , (select sum(qta) from store_udc_dett where imperfezioni is not null and codice = d.codicedipa) as DispImperfezioni
  , D.QuantitaInCarrello AS InCarrello, format(lp.Costo, 'C') as Costo, D.QuantitaDaEvadere AS InArrivo  
  , 0.1 * (CASE WHEN (SELECT Max(v) FROM (VALUES (ISNULL(P.ScortaMinima,0)),(ISNULL(P.ScortaMinimaMedia90,0)),(ISNULL(P.ScortaMinimaIstantanea,0))) AS value(v)) * (100 + 0) / 100.0 - (ISNULL(D.Disponibilita,0)+ISNULL(D.QuantitaDaEvadere,0)) < 0 THEN 0 
    ELSE ((SELECT Max(v) FROM (VALUES (ISNULL(P.ScortaMinima,0)),(ISNULL(P.ScortaMinimaMedia90,0)),(ISNULL(P.ScortaMinimaIstantanea,0))) AS value(v)) * (100 + 0)/ 10 - 10 * (ISNULL(D.Disponibilita,0)+ISNULL(D.QuantitaDaEvadere,0))) END) AS DaOrdinare  
  , P.ScortaMinimaIgnora, P.ScortaMinima,P.ScortaMinimaMedia90,P.ScortaMinimaIstantanea, P.Disponibilita48Ore, P.Disponibilita48OreRef  
  , (CASE WHEN V.Venduto_360 IS NULL OR V.Venduto_360 * 0.2 < 2 THEN 2 ELSE Round(V.Venduto_360 * 0.2, 0) END) AS ScortaMinimaCalcolata  
  , P.ScortaMinimaNoRicalcoloAutomatico, P.RegolaDisponibilita, V.Venduto_30, V.Venduto_60, V.Venduto_90, V.Venduto_180, V.Venduto_360, V.Venduto_Medio_Mese  
  , VM.Venduto_Mese_0  , VM.Venduto_Mese_1  , VM.Venduto_Mese_2  , VM.Venduto_Mese_3  , VM.Venduto_Mese_4  , VM.Venduto_Mese_5  , VM.Venduto_Mese_6  , VM.Venduto_Mese_7  , VM.Venduto_Mese_8  , VM.Venduto_Mese_9  , VM.Venduto_Mese_10  , VM.Venduto_Mese_11  , VM.Venduto_Mese_12  
  , (SELECT COUNT(*) FROM ProdottiFornitori WHERE ProdottiFornitori.CodiceDiPa=P.CodiceDiPa) AS NumRif  
  , Pr.CodiceOEM, Pr.CodiciOriginali, [dbo].[codici_compatibili] (p.CodiceDiPa) as Compatibili 
 FROM PRODOTTI AS Pr with(nolock)  
 INNER JOIN PRODOTTI_SCORTEDISPONIBILITA AS P with(nolock) ON Pr.product_id = P.product_id  
 INNER JOIN PRODOTTI_DISPONIBILITA AS D with(nolock) ON P.product_id = D.product_id  
 INNER JOIN PRODOTTI_VENDITE AS V with(nolock) ON P.product_id = V.product_id  
 INNER JOIN PRODOTTI_VENDITE_MESE AS VM with(nolock) ON P.product_id=VM.product_id 
 LEFT JOIN LISTINIPRODOTTI lp with(nolock) on lp.codicedipa = pr.codicedipa  
 WHERE 1 = 1 and pr.disabled = 0 
 ORDER BY P.CodiceDiPa



 -- carico i prodotti fornitori
SELECT * FROM (
  SELECT DISTINCT TOP 100 'PF' as Tipo, PF.CodiceDiPa, F.Fornitore, F.cod_cf as cod_f
        , (case when left(pf.FornitoreMeseAnnoPrezzo, 4) = 'ORD ' then pf.FornitoreMeseAnnoPrezzo else '' end) as NroOrdine, f.Note as NoteFornitore
        , cast(F.NoOrdine as bit) as NoOrdine, P.Produttore, P.cod_cf as cod_p, PF.ProduttoreCodiceClean as ProduttoreCodice
        , case when len(pr.CodiceDiPa) = 0  or pr.CodiceDiPa is null then pf.CodiceOem else pr.codiceoem end as CodiceOem
        , case when len(pr.CodiceDiPa) = 0  or pr.CodiceDiPa is null then Concat(pf.Marchio,' - ',pf.ProduttoreNoteCodice) else pf.ProduttoreNoteCodice end as ProduttoreNoteCodice
        , PF.FornitoreProdottoNonDisponibile, PF.FornitoreMeseAnnoPrezzo, convert(varchar, PF.FornitoreDataOreModifica, 111) as AnnoMesePrezzo
        , 0 as Qta, PF.FornitorePrezzo, '' as Preventivo, '' as IdPreventivo, PF.FornitoreDestinatarioPrezzo
        , PF.FornitoreNotePrezzo, PF.IDFornitore, PF.IDProduttore, PF.FornitorePrezzoStoricizzato
        , case when len(pr.CodiceDiPa) = 0 or pr.CodiceDiPa is null then pf.NR else '' end as Desinenza, PF.Note
      FROM ProdottiFornitori AS PF with(nolock)
      LEFT join prodotti pr with(nolock) on pr.codicedipa = pf.codicedipa
      LEFT JOIN VW_Fornitori AS F with(nolock) ON PF.IDFornitore = F.IDFornitore
      LEFT JOIN VW_Produttori AS P with(nolock) ON PF.IDProduttore = P.IDProduttore
      WHERE 1 = 1 and (pr.disabled = 0 or pr.disabled is null) 
       AND (PF.codicedipa LIKE 'egr01%' OR PF.produttorecodiceclean LIKE 'egr01%' 
        OR exists (SELECT top 1 1 FROM Prodotti with(nolock) WHERE CodiceDiPa = PF.codicedipa and codiceoem like 'egr01%') 
        OR exists (select top 1 1 from prodoem_det with(nolock) where codicedipa = PF.codicedipa and codiceoem like 'egr01%'))

 union all SELECT DISTINCT TOP 100 'QR' as Tipo, QC.cod_dipa as CodiceDiPa, UPPER(QC.e_commerce) as Fornitore, f.cod_cf as cod_f
        , '' as NroOrdine, f.Note as NoteFornitore, '' as NoOrdine, UPPER(QC.manufacturer) as Produttore, p.cod_cf as cod_p, QC.code_clean as ProduttoreCodice
        , case when len(pr.codiceoem) = 0  or pr.codiceoem is null then QC.from_code_search else pr.codiceoem end as CodiceOem
        , CONCAT(UPPER(QC.manufacturer),' - ',QC.description,' disp.',QC.av_desc) as ProduttoreNoteCodice
        , 0 as FornitoreProdottoNonDisponibile, cast(FORMAT(QC.dt_ins,'yyyy/MM/dd') as varchar(10)) as FornitoreMeseAnnoPrezzo
        , cast(FORMAT(QC.dt_ins,'yyyy/MM/dd') as varchar(10)) as AnnoMesePrezzo, 0 as Qta, QC.price as FornitorePrezzo
        , QC.cod_gest as Preventivo, QC.id_preventivo as IdPreventivo, '' as FornitoreDestinatarioPrezzo
        , '' as FornitoreNotePrezzo, f.IDFornitore as IDFornitore, p.IDProduttore as IDProduttore
        , 0 as FornitorePrezzoStoricizzato, '' as Desinenza, '' as Note
      FROM VW_QR_CODES_ACQ AS QC with(nolock)
      left join prodotti pr with(nolock) on pr.codicedipa = QC.cod_dipa
      left join VW_FORNITORI f on f.Fornitore = QC.e_commerce 
      left join VW_PRODUTTORI p on p.Produttore = LOWER(QC.manufacturer) 
      WHERE 1 = 1 and (pr.disabled = 0 or pr.disabled is null) AND (QC.cod_dipa LIKE 'egr01%' OR QC.code_clean LIKE 'egr01%' OR QC.from_code_search LIKE 'egr01%')

  union all SELECT DISTINCT TOP 100 'ACQ' as Tipo, vdf.cod_dipa as CodiceDiPa, UPPER(f.Fornitore) as Fornitore, f.cod_cf as cod_f
        , vdf.numreg as NroOrdine, f.Note as NoteFornitore, '' as NoOrdine, UPPER(f.Fornitore) as Produttore, f.cod_cf as cod_p
        , vdf.cod_forn as ProduttoreCodice, pr.codiceoem as CodiceOem, '' as ProduttoreNoteCodice, 0 as FornitoreProdottoNonDisponibile
        , cast(FORMAT(vdf.datadoc ,'yyyy/MM/dd') as varchar(10)) as FornitoreMeseAnnoPrezzo
        , cast(FORMAT(vdf.datadoc ,'yyyy/MM/dd') as varchar(10)) as AnnoMesePrezzo, vdf.Qta, vdf.prezzo as FornitorePrezzo
        , '' as Preventivo, '' as IdPreventivo, '' as FornitoreDestinatarioPrezzo, '' as FornitoreNotePrezzo, f.IDFornitore as IDFornitore
        , p.IDProduttore as IDProduttore, 0 as FornitorePrezzoStoricizzato, '' as Desinenza, '' as Note
      FROM VW_DETT_FORN AS vdf with(nolock)
      join prodotti pr with(nolock) on pr.codicedipa = vdf.cod_dipa
      left join VW_FORNITORI f on f.cod_cf = vdf.cod_cf
      left join VW_PRODUTTORI p on p.Produttore = LOWER(vdf.ragsoc) 
      WHERE 1 = 1 and pr.disabled = 0 AND (vdf.cod_dipa LIKE 'egr01%' OR vdf.code_clean LIKE 'egr01%' 
       OR exists (SELECT top 1 1 FROM Prodotti with(nolock) WHERE CodiceDiPa = vdf.cod_dipa and codiceoem like '%egr01%') 
       OR exists (select top 1 1 from prodoem_det with(nolock) where codicedipa = vdf.cod_dipa and codiceoem like '%egr01%'))

) as VW

 ORDER BY VW.Tipo,VW.AnnoMesePrezzo DESC

 select f.fornitore, pf.* from ProdottiFornitori pf
  join VW_FORNITORI f on f.IDFornitore = pf.IDFornitore
  where f.Fornitore like 'cont%'

  VW_FORNITORI where cod_cf = '001599'
  VW_FORNITORI where cod_cf = '001699'

  VW_FORNITORI order by 2
  clifor_f where ragsoc like '%continental%'
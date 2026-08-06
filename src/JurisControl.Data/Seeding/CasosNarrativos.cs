using JurisControl.Domain.Enums;

namespace JurisControl.Data.Seeding;

/// <summary>
/// Casos featured con narrativa procesal completa desde la demanda hasta la
/// segunda instancia. El DemoDataSeeder crea el juicio y le encadena
/// actuaciones y promociones con textos jurídicos ricos en las fechas
/// relativas indicadas (offset en días desde FechaInicio).
/// </summary>
public static class CasosNarrativos
{
    public sealed record ActuacionNar(int OffsetDias, TipoActuacion Tipo, string Resumen, string Detalle);
    public sealed record PromocionNar(int OffsetDias, TipoPromocion Tipo, string Titulo, string Contenido);
    public sealed record CasoNar(
        string Materia,
        string TipoJuicio,
        string Juzgado,
        string Titulo,
        string Descripcion,
        string NombreActor,
        string NombreDemandado,
        decimal Cuantia,
        EstadoJuicio EstadoJuicioFinal,
        EstadoAsunto EstadoAsuntoFinal,
        int DiasInicioAtras,
        ActuacionNar[] Actuaciones,
        PromocionNar[] Promociones);

    public static readonly CasoNar[] Casos =
    {
        // ============================================================
        // CASO 1 — Ejecutivo Mercantil por pagaré (COMPLETO hasta 2a instancia)
        // ============================================================
        new CasoNar(
            Materia.Mercantil,
            "Ejecutivo Mercantil",
            "Juzgado 6° de lo Mercantil, CDMX",
            "Ejecutivo Mercantil vs. Distribuidora Norte del Golfo — pagaré vencido",
            "Ejecución de pagaré por MXN 850,000 suscrito por el representante legal de la parte demandada, con vencimiento del 15 de octubre de 2024. Se solicita el pago de suerte principal, intereses moratorios pactados al 4% mensual y costas.",
            "Grupo Corporativo Alameda S.A.P.I.",
            "Distribuidora Norte del Golfo S.A. de C.V.",
            850_000m,
            EstadoJuicio.Apelacion,
            EstadoAsunto.Activo,
            DiasInicioAtras: 480,
            Actuaciones: new[]
            {
                new ActuacionNar(0, TipoActuacion.Acuerdo,
                    "Auto que admite la demanda en la vía ejecutiva mercantil.",
                    "Se admite la demanda en la vía ejecutiva mercantil. Se despacha auto de ejecución. Se manda requerir de pago a la parte demandada por la cantidad de $850,000.00 (ochocientos cincuenta mil pesos 00/100 M.N.) por concepto de suerte principal, más los intereses moratorios pactados al 4% mensual desde la fecha de vencimiento. Se ordena el embargo en su caso sobre bienes suficientes para garantizar las cantidades reclamadas y sus accesorios."),
                new ActuacionNar(8, TipoActuacion.Diligencia,
                    "Diligencia de requerimiento de pago y embargo.",
                    "Constituido el C. Actuario en el domicilio señalado, se requirió a la parte demandada el pago de las cantidades reclamadas. Al no haberse hecho el pago, se procedió a trabar embargo sobre 3 vehículos utilitarios propiedad de la empresa demandada, mismos que quedaron a disposición del juzgado en calidad de depositario judicial el representante legal, quien firmó de conformidad."),
                new ActuacionNar(9, TipoActuacion.Notificacion,
                    "Emplazamiento a juicio a la parte demandada.",
                    "En el mismo acto se emplazó a juicio a la parte demandada, entregándole cédula de notificación y copias de traslado del escrito inicial de demanda y anexos. Se le concede el término de OCHO DÍAS para contestar."),
                new ActuacionNar(30, TipoActuacion.Acuerdo,
                    "Auto que tiene por contestada la demanda oponiendo excepciones.",
                    "Se tiene a la parte demandada por contestada la demanda dentro del término legal, oponiendo las excepciones de PAGO PARCIAL, NOVACIÓN y FALTA DE PERSONALIDAD del suscriptor del pagaré. Se manda dar vista a la parte actora por el término de TRES DÍAS."),
                new ActuacionNar(38, TipoActuacion.Acuerdo,
                    "Auto que abre el juicio a prueba por 40 días.",
                    "Habiéndose desahogado la vista, se abre el juicio a prueba por el término común de CUARENTA DÍAS HÁBILES para que las partes ofrezcan sus pruebas."),
                new ActuacionNar(55, TipoActuacion.Acuerdo,
                    "Auto que admite pruebas ofrecidas por las partes.",
                    "Se admiten como pruebas de la parte actora: 1) La documental pública consistente en el pagaré base de la acción; 2) La confesional a cargo del representante legal de la demandada; 3) La testimonial a cargo de dos testigos. De la parte demandada se admiten: 1) La documental relativa a los recibos de pago parcial; 2) La pericial en grafoscopía sobre la firma del pagaré."),
                new ActuacionNar(85, TipoActuacion.Audiencia,
                    "Se celebra audiencia de desahogo de la confesional y testimonial.",
                    "Con la asistencia del apoderado legal de la parte actora y del absolvente en representación de la demandada, se desahogó la prueba confesional en 12 posiciones, resultando favorables a la parte actora las posiciones 3, 5, 7, 8 y 11. Se desahogó igualmente la testimonial a cargo de los CC. testigos, quienes rindieron su declaración y fueron interrogados por ambas partes."),
                new ActuacionNar(120, TipoActuacion.Diligencia,
                    "Dictamen pericial en grafoscopía rendido.",
                    "El perito tercero en discordia designado por el juzgado, C. Perito Grafoscópico, rinde su dictamen, concluyendo que la firma estampada en el pagaré base de la acción SÍ CORRESPONDE al puño y letra del representante legal de la parte demandada. Con lo anterior queda desvirtuada la excepción de falta de personalidad."),
                new ActuacionNar(150, TipoActuacion.Acuerdo,
                    "Se cita a las partes para formular alegatos.",
                    "Habiéndose desahogado todas las pruebas admitidas, se cita a las partes a formular sus alegatos por escrito dentro del término de CINCO DÍAS HÁBILES."),
                new ActuacionNar(180, TipoActuacion.Sentencia,
                    "SENTENCIA DEFINITIVA de primera instancia — favorable a la parte actora.",
                    "PRIMERO. La parte actora ha probado su acción cambiaria directa. La demandada no acreditó sus excepciones. SEGUNDO. Se condena a la parte demandada al pago de $850,000.00 por suerte principal, más los intereses moratorios pactados al 4% mensual desde el 15 de octubre de 2024 hasta su total liquidación, así como al pago de las costas del juicio. TERCERO. Notifíquese personalmente y por Boletín Judicial."),
                new ActuacionNar(190, TipoActuacion.Notificacion,
                    "Notificación personal de la sentencia definitiva.",
                    "Constituido el C. Actuario en el domicilio de la parte demandada, se le notificó personalmente el contenido íntegro de la sentencia definitiva, entregándole cédula. Se le hace saber el término de NUEVE DÍAS HÁBILES para interponer recurso de apelación."),
                new ActuacionNar(215, TipoActuacion.Resolucion,
                    "Auto que admite el recurso de apelación en efecto devolutivo.",
                    "Se admite en el efecto DEVOLUTIVO el recurso de apelación interpuesto por la parte demandada en contra de la sentencia definitiva. Remítanse los autos a la H. Sala Civil que en turno corresponda para su substanciación."),
                new ActuacionNar(270, TipoActuacion.Acuerdo,
                    "H. Sala Civil recibe autos y radica la apelación.",
                    "La H. Cuarta Sala Civil del Tribunal Superior de Justicia de la CDMX recibe los autos y admite el trámite de la apelación. Se cita a audiencia de vista para dentro de TREINTA DÍAS.")
            },
            Promociones: new[]
            {
                new PromocionNar(0, TipoPromocion.Demanda,
                    "Escrito inicial de demanda ejecutiva mercantil.",
                    "Se acompaña como documento base de la acción el pagaré original suscrito por el representante legal de la parte demandada. Se solicitan las siguientes prestaciones: 1) El pago de la suerte principal por $850,000.00; 2) El pago de los intereses moratorios pactados al 4% mensual desde el 15 de octubre de 2024; 3) El pago de las costas del juicio."),
                new PromocionNar(25, TipoPromocion.Contestacion,
                    "Vista a la contestación de demanda.",
                    "Se objetan las excepciones opuestas por la parte demandada. Sobre el pago parcial, se manifiesta que los recibos exhibidos son apócrifos. Sobre la novación, no existe convenio modificatorio alguno. Sobre la falta de personalidad, se pide se ordene la pericial correspondiente."),
                new PromocionNar(45, TipoPromocion.Ofrecimiento,
                    "Ofrecimiento de pruebas.",
                    "Se ofrecen las siguientes pruebas: documental pública (pagaré), confesional (representante legal), testimonial (dos testigos que presenciaron la suscripción del pagaré), y todas las presunciones que se deriven de autos."),
                new PromocionNar(165, TipoPromocion.Alegatos,
                    "Alegatos de bien probado.",
                    "Ha quedado plenamente acreditado que la parte demandada suscribió el pagaré base de la acción, que la firma corresponde a su representante legal según el dictamen pericial rendido, y que las excepciones opuestas no fueron probadas. Procede dictar sentencia condenatoria."),
                new PromocionNar(230, TipoPromocion.Recurso,
                    "Escrito de agravios en apelación.",
                    "Se agravia la parte apelante en cuanto a la valoración de las pruebas y la desestimación de las excepciones opuestas. Se pide se revoque la sentencia definitiva y se absuelva a la demandada.")
            }),

        // ============================================================
        // CASO 2 — Ordinario Civil (arrendamiento con desahucio)
        // ============================================================
        new CasoNar(
            Materia.Civil,
            "Especial de Desahucio",
            "Juzgado 15° de lo Civil, CDMX",
            "Desahucio vs. inquilino moroso — Depto Colonia Roma",
            "Rescisión del contrato de arrendamiento por falta de pago de 8 rentas vencidas del inmueble ubicado en la Colonia Roma Norte, así como el pago de rentas devengadas y las que se sigan generando hasta la entrega del inmueble.",
            "Inmobiliaria Reforma 2000 S.A. de C.V.",
            "María Guadalupe Vázquez Reyes",
            185_000m,
            EstadoJuicio.Ejecucion,
            EstadoAsunto.Activo,
            DiasInicioAtras: 300,
            Actuaciones: new[]
            {
                new ActuacionNar(0, TipoActuacion.Acuerdo,
                    "Auto que admite la demanda de desahucio.",
                    "Se admite la demanda en la vía especial de desahucio. Se manda emplazar a la parte demandada, concediéndole el término de CINCO DÍAS para desalojar el inmueble o dar contestación con documentos que acrediten el pago."),
                new ActuacionNar(6, TipoActuacion.Diligencia,
                    "Diligencia de emplazamiento y requerimiento.",
                    "El C. Actuario se constituyó en el inmueble arrendado. Se entendió la diligencia con la parte demandada, a quien se emplazó personalmente entregándole cédula y copias de traslado. Se le requirió para el pago de rentas vencidas o el desalojo dentro del término legal."),
                new ActuacionNar(20, TipoActuacion.Acuerdo,
                    "Contestación de demanda sin exhibir documentos de pago.",
                    "Se tiene por contestada la demanda. La parte demandada niega parcialmente los hechos pero no exhibe documento alguno que acredite el pago de las rentas reclamadas. Se abre el juicio a prueba por DIEZ DÍAS."),
                new ActuacionNar(40, TipoActuacion.Acuerdo,
                    "Auto que admite pruebas.",
                    "Se admiten como pruebas de la actora: contrato de arrendamiento, recibos no pagados, testimonial del administrador del edificio. De la demandada: confesional a cargo del representante legal de la actora."),
                new ActuacionNar(65, TipoActuacion.Audiencia,
                    "Audiencia de pruebas y alegatos celebrada.",
                    "Se desahogaron las pruebas admitidas. El administrador del edificio confirmó bajo protesta de decir verdad que la demandada adeuda 8 mensualidades. Las partes formularon alegatos verbales."),
                new ActuacionNar(95, TipoActuacion.Sentencia,
                    "SENTENCIA DEFINITIVA — condenatoria.",
                    "PRIMERO. Se declara rescindido el contrato de arrendamiento. SEGUNDO. Se condena a la demandada a desocupar y entregar el inmueble en el término de TREINTA DÍAS. TERCERO. Al pago de $185,000.00 por rentas vencidas más las que se sigan generando hasta la entrega. CUARTO. Costas del juicio."),
                new ActuacionNar(140, TipoActuacion.Resolucion,
                    "Sentencia declarada ejecutoriada por no haberse interpuesto recurso.",
                    "Habiendo transcurrido el término para interponer recurso de apelación sin que se hubiere hecho valer, se declara ejecutoriada la sentencia definitiva."),
                new ActuacionNar(165, TipoActuacion.Diligencia,
                    "Diligencia de lanzamiento y entrega del inmueble.",
                    "El C. Actuario se constituyó en el inmueble acompañado del cerrajero. Se procedió al lanzamiento de la parte demandada, y se hizo entrega material del inmueble a la parte actora, previa inspección del estado del mismo."),
                new ActuacionNar(180, TipoActuacion.Acuerdo,
                    "Se ordena la liquidación de costas y sentencia.",
                    "Presentada por la actora la planilla de liquidación de costas y sentencia por un total de $215,300.00, se ordena dar vista a la contraparte por tres días.")
            },
            Promociones: new[]
            {
                new PromocionNar(0, TipoPromocion.Demanda,
                    "Demanda de desahucio.",
                    "Se demanda la rescisión del contrato de arrendamiento por falta de pago de 8 rentas vencidas, el desalojo del inmueble y el pago de las cantidades adeudadas."),
                new PromocionNar(50, TipoPromocion.Ofrecimiento,
                    "Ofrecimiento y desahogo de pruebas.",
                    "Se acompañan el contrato de arrendamiento y los recibos de las 8 rentas no pagadas. Se ofrece testimonial del administrador del edificio."),
                new PromocionNar(150, TipoPromocion.Otro,
                    "Se solicita el lanzamiento por sentencia ejecutoriada.",
                    "En virtud de haber quedado ejecutoriada la sentencia y transcurrido el término concedido para la entrega voluntaria del inmueble, se solicita se ordene el lanzamiento con el auxilio del C. Actuario y el uso de la fuerza pública si fuere necesario."),
                new PromocionNar(175, TipoPromocion.Otro,
                    "Planilla de liquidación de costas y sentencia.",
                    "Se presenta planilla que asciende a $215,300.00 por concepto de rentas vencidas, rentas devengadas hasta la fecha de entrega, y costas del juicio.")
            }),

        // ============================================================
        // CASO 3 — Amparo indirecto contra clausura administrativa
        // ============================================================
        new CasoNar(
            Materia.Amparo,
            "Amparo Indirecto",
            "Juzgado 4° de Distrito en Materia Administrativa, CDMX",
            "Amparo contra clausura del establecimiento comercial",
            "Se promueve amparo indirecto contra la clausura del establecimiento comercial ordenada por el INVEA CDMX, por presuntas violaciones a los artículos 14 y 16 constitucionales al no haberse otorgado garantía de audiencia previa.",
            "Bebidas Artesanales Cuauhtémoc S.A.",
            "Instituto de Verificación Administrativa CDMX",
            0m,
            EstadoJuicio.Sentencia,
            EstadoAsunto.Activo,
            DiasInicioAtras: 210,
            Actuaciones: new[]
            {
                new ActuacionNar(0, TipoActuacion.Acuerdo,
                    "Auto que admite la demanda de amparo y concede suspensión provisional.",
                    "Se admite la demanda de amparo indirecto. Se concede la suspensión provisional del acto reclamado, para el efecto de que las cosas se mantengan en el estado que guardan y no se ejecute la clausura hasta en tanto se resuelva sobre la suspensión definitiva."),
                new ActuacionNar(2, TipoActuacion.Notificacion,
                    "Notificación al C. Juez de Distrito de la suspensión provisional.",
                    "Se notifica a la autoridad responsable la concesión de la suspensión provisional para que se abstenga de ejecutar la clausura."),
                new ActuacionNar(15, TipoActuacion.Audiencia,
                    "Audiencia incidental para suspensión definitiva.",
                    "Con vista al informe justificado rendido por la autoridad responsable, se celebra la audiencia incidental. Se concede la SUSPENSIÓN DEFINITIVA del acto reclamado, condicionada al otorgamiento de garantía por $50,000.00 para responder de los daños al tercero interesado."),
                new ActuacionNar(45, TipoActuacion.Acuerdo,
                    "Se tienen por rendidos los informes justificados.",
                    "Las autoridades responsables rinden en tiempo sus informes justificados sosteniendo la legalidad del acto reclamado. Se manda dar vista a la parte quejosa."),
                new ActuacionNar(90, TipoActuacion.Audiencia,
                    "Audiencia constitucional.",
                    "Se celebra la audiencia constitucional con la asistencia del Ministerio Público de la Federación adscrito, quien formula pedimento en el sentido de que se conceda el amparo. Se cierra la instrucción y se cita para sentencia."),
                new ActuacionNar(150, TipoActuacion.Sentencia,
                    "SENTENCIA CONSTITUCIONAL — AMPARA Y PROTEGE.",
                    "ÚNICO. La Justicia de la Unión AMPARA y PROTEGE a la parte quejosa en contra del acto reclamado consistente en la orden de clausura, para el efecto de que la autoridad responsable la deje insubsistente y, en su lugar, siga el procedimiento administrativo previsto en la Ley respectiva, respetando la garantía de audiencia previa. Notifíquese.")
            },
            Promociones: new[]
            {
                new PromocionNar(0, TipoPromocion.Amparo,
                    "Demanda de amparo indirecto.",
                    "Se promueve juicio de amparo indirecto contra los actos consistentes en: 1) La orden de clausura del establecimiento; 2) Su ejecución por el personal verificador del INVEA. Se señalan como preceptos violados los artículos 14 y 16 constitucionales. Se pide la suspensión provisional y definitiva del acto reclamado."),
                new PromocionNar(20, TipoPromocion.Otro,
                    "Exhibe garantía para suspensión definitiva.",
                    "Se acompaña póliza de fianza número F-2025-123456 emitida por Afianzadora Mexicana S.A. por la cantidad de $50,000.00 para responder de los daños y perjuicios que pudieren ocasionarse al tercero interesado con motivo de la suspensión concedida."),
                new PromocionNar(55, TipoPromocion.Otro,
                    "Objeción a los informes justificados.",
                    "Se objetan los informes justificados rendidos por las autoridades responsables. Los actos reclamados son inconstitucionales porque no se cumplió con la garantía de audiencia previa que exige el artículo 14 constitucional.")
            }),

        // ============================================================
        // CASO 4 — Divorcio necesario (Bufete Álvarez)
        // ============================================================
        new CasoNar(
            Materia.Familiar,
            "Divorcio Necesario",
            "Juzgado 12° de lo Familiar, CDMX",
            "Divorcio con controversia sobre guarda y custodia",
            "Divorcio necesario invocando las causales de sevicia, malos tratos y abandono. Se solicita la guarda y custodia de los dos menores hijos, así como pensión alimenticia y liquidación de sociedad conyugal.",
            "Patricia González Rivera",
            "Roberto Hernández Sánchez",
            0m,
            EstadoJuicio.EnPruebas,
            EstadoAsunto.Activo,
            DiasInicioAtras: 150,
            Actuaciones: new[]
            {
                new ActuacionNar(0, TipoActuacion.Acuerdo,
                    "Auto que admite la demanda y dicta medidas provisionales.",
                    "Se admite la demanda de divorcio necesario. Como medidas provisionales, se ordena: 1) La guarda y custodia provisional de los menores a favor de la parte actora; 2) Se fija pensión alimenticia provisional del 40% del sueldo del demandado a favor de los menores; 3) Régimen de convivencias los fines de semana alternos."),
                new ActuacionNar(15, TipoActuacion.Diligencia,
                    "Emplazamiento al cónyuge demandado.",
                    "Se emplazó personalmente al C. demandado en su domicilio laboral, concediéndole el término de NUEVE DÍAS para contestar la demanda y ofrecer pruebas."),
                new ActuacionNar(35, TipoActuacion.Acuerdo,
                    "Contestación oponiendo reconvención.",
                    "El demandado contesta en tiempo y hace valer reconvención, en la cual reclama para sí la guarda y custodia de los menores, argumentando que la parte actora los ha alienado. Se manda dar vista."),
                new ActuacionNar(70, TipoActuacion.Acuerdo,
                    "Auto que admite pruebas y ordena estudios psicológicos.",
                    "Se admiten las pruebas ofrecidas por ambas partes. Como pruebas ordenadas por el Juzgado, se decreta la práctica de estudios psicológicos a los menores y a ambos progenitores por parte del CTA (Centro de Convivencia Familiar).")
            },
            Promociones: new[]
            {
                new PromocionNar(0, TipoPromocion.Demanda,
                    "Demanda de divorcio necesario.",
                    "Se demanda el divorcio invocando las causales previstas en el artículo 267 fracciones X y XI del Código Civil para el Distrito Federal. Se pide la guarda y custodia de los menores, pensión alimenticia y liquidación de la sociedad conyugal."),
                new PromocionNar(55, TipoPromocion.Contestacion,
                    "Contestación a la reconvención.",
                    "Se contesta la reconvención negando categóricamente los hechos imputados. Los menores han manifestado su deseo de continuar bajo el cuidado de la madre. Se ofrece prueba pericial en psicología.")
            }),

        // ============================================================
        // CASO 5 — Cobranza bancaria (Corporativo Jurídico Reforma)
        // ============================================================
        new CasoNar(
            Materia.Cobranza,
            "Ejecutivo Mercantil con garantía hipotecaria",
            "Juzgado 10° de lo Mercantil, CDMX",
            "Ejecución de crédito hipotecario — Banco del Norte vs. Guerrero Ortiz",
            "Ejecución de crédito con garantía hipotecaria por MXN 2,400,000 vencido y no pagado. Se solicita el remate del inmueble hipotecado ubicado en Colonia Del Valle.",
            "Banco del Norte S.A., Institución de Banca Múltiple",
            "Fernando Guerrero Ortiz",
            2_400_000m,
            EstadoJuicio.Ejecucion,
            EstadoAsunto.Activo,
            DiasInicioAtras: 400,
            Actuaciones: new[]
            {
                new ActuacionNar(0, TipoActuacion.Acuerdo,
                    "Auto admisorio y despacho de ejecución con embargo.",
                    "Se admite la demanda en la vía ejecutiva mercantil. Se despacha auto de ejecución con embargo sobre el inmueble hipotecado inscrito bajo el folio real electrónico 1234567 del Registro Público de la Propiedad de la CDMX."),
                new ActuacionNar(7, TipoActuacion.Diligencia,
                    "Diligencia de requerimiento, embargo y emplazamiento.",
                    "Se requirió al demandado el pago de $2,400,000.00 más accesorios. Ante la falta de pago, se ratificó el embargo sobre el inmueble hipotecado. Se le emplazó personalmente."),
                new ActuacionNar(30, TipoActuacion.Acuerdo,
                    "Rebeldía del demandado. Se declara precluído el derecho.",
                    "Habiendo transcurrido el término del emplazamiento sin que la parte demandada hubiere contestado, se le acusa la correspondiente rebeldía y se le declara precluído su derecho para hacerlo. Se abre el juicio a prueba."),
                new ActuacionNar(75, TipoActuacion.Sentencia,
                    "SENTENCIA DEFINITIVA — condena y remate.",
                    "Se declara procedente la acción hipotecaria. Se condena al demandado al pago del capital, intereses ordinarios, moratorios y costas. Se ordena la venta en pública subasta del inmueble hipotecado con la base del avalúo pericial."),
                new ActuacionNar(130, TipoActuacion.Diligencia,
                    "Practicado avalúo del inmueble hipotecado.",
                    "El perito valuador designado por el juzgado rinde su dictamen fijando el valor comercial del inmueble en $3,850,000.00. Se ordena que la base del remate sea dicha cantidad."),
                new ActuacionNar(165, TipoActuacion.Acuerdo,
                    "Se señala fecha para primera almoneda.",
                    "Se señalan las 10:00 horas del día 15 de septiembre de 2026 para que tenga verificativo la primera almoneda del inmueble hipotecado. Publíquense los edictos correspondientes en el Boletín Judicial y en un periódico de circulación amplia.")
            },
            Promociones: new[]
            {
                new PromocionNar(0, TipoPromocion.Demanda,
                    "Demanda ejecutiva mercantil con garantía hipotecaria.",
                    "Se acompañan como documentos base: 1) El contrato de apertura de crédito con garantía hipotecaria; 2) El estado de cuenta certificado por contador facultado; 3) La escritura pública que contiene la constitución de la hipoteca inscrita en el Registro Público de la Propiedad. Se pide la venta judicial del inmueble."),
                new PromocionNar(70, TipoPromocion.Alegatos,
                    "Alegatos ante la rebeldía.",
                    "Se alega que ha quedado plenamente probada la acción ejecutiva hipotecaria con los documentos base exhibidos, y que la rebeldía del demandado configura confesión ficta de los hechos aducidos en la demanda."),
                new PromocionNar(120, TipoPromocion.Otro,
                    "Solicita se ordene avalúo del inmueble hipotecado.",
                    "Se solicita se ordene la práctica del avalúo pericial del inmueble hipotecado para efectos del remate."),
                new PromocionNar(180, TipoPromocion.Otro,
                    "Publicación de edictos para la primera almoneda.",
                    "Se solicita se ordene la publicación de los edictos convocando postores para la primera almoneda del inmueble.")
            }),

        // ============================================================
        // CASO 6 — Laboral (Bufete Álvarez)
        // ============================================================
        new CasoNar(
            Materia.Laboral,
            "Ordinario Laboral",
            "Juzgado 2° del Trabajo, Centro Federal, CDMX",
            "Despido injustificado y prestaciones — ex-empleado vs. empresa",
            "Reclamo por despido injustificado, pago de indemnización constitucional, salarios caídos, aguinaldo, vacaciones y prima vacacional proporcionales, así como reparto de utilidades del último ejercicio.",
            "José Luis Torres Martínez",
            "Textiles Industriales de México S.A.",
            420_000m,
            EstadoJuicio.Concluido,
            EstadoAsunto.Cerrado,
            DiasInicioAtras: 540,
            Actuaciones: new[]
            {
                new ActuacionNar(0, TipoActuacion.Acuerdo,
                    "Radicación de la demanda y señalamiento de audiencia inicial.",
                    "Se admite la demanda laboral. Se señalan las 10:00 horas del día que corresponda para que tenga verificativo la audiencia de conciliación, demanda y excepciones, ofrecimiento y admisión de pruebas."),
                new ActuacionNar(45, TipoActuacion.Audiencia,
                    "Audiencia inicial — sin conciliación. Se traba la litis.",
                    "Las partes comparecen. No hay ánimo conciliatorio. La parte demandada ofrece 3 meses de salario por cesantía voluntaria. La actora rechaza. Se ratifica la demanda y se contesta oponiendo excepciones."),
                new ActuacionNar(120, TipoActuacion.Audiencia,
                    "Audiencia de desahogo de pruebas.",
                    "Se desahogan las pruebas ofrecidas: confesional, testimonial y documental. Los testigos ofrecidos por la actora confirman que fue despedido de manera verbal por el gerente general el día 15 de febrero de 2025."),
                new ActuacionNar(240, TipoActuacion.Sentencia,
                    "LAUDO — condenatorio para la parte patronal.",
                    "PRIMERO. Se acredita el despido injustificado. SEGUNDO. Se condena a la demandada al pago de: 3 meses de salario por indemnización constitucional ($120,000.00), 20 días de salario por cada año trabajado ($180,000.00), salarios caídos ($90,000.00), aguinaldo y vacaciones proporcionales ($30,000.00). Total: $420,000.00."),
                new ActuacionNar(280, TipoActuacion.Resolucion,
                    "Laudo declarado firme.",
                    "Habiendo transcurrido el término sin que se hubiere interpuesto amparo directo, se declara firme el laudo condenatorio."),
                new ActuacionNar(320, TipoActuacion.Diligencia,
                    "Pago total del laudo. Convenio y desistimiento.",
                    "La parte demandada realiza el pago total del laudo mediante cheque de caja por $420,000.00. La parte actora firma convenio finiquito y otorga el más amplio finiquito de ley. Se archiva el expediente como asunto concluido.")
            },
            Promociones: new[]
            {
                new PromocionNar(0, TipoPromocion.Demanda,
                    "Escrito inicial de demanda laboral.",
                    "El actor manifiesta haber sido despedido injustificadamente el 15 de febrero de 2025 después de 12 años de servicio ininterrumpido con un salario mensual de $40,000.00. Reclama indemnización constitucional y demás prestaciones legales."),
                new PromocionNar(100, TipoPromocion.Ofrecimiento,
                    "Ofrecimiento de pruebas.",
                    "Se ofrecen las pruebas: confesional a cargo del representante legal; testimonial a cargo de 3 compañeros de trabajo; documental consistente en recibos de nómina de los últimos 12 meses."),
                new PromocionNar(200, TipoPromocion.Alegatos,
                    "Alegatos de bien probado.",
                    "Ha quedado acreditada la relación laboral, la antigüedad, el salario y el despido injustificado. Procede condenar a la demandada al pago íntegro de las prestaciones reclamadas."),
                new PromocionNar(315, TipoPromocion.Otro,
                    "Convenio finiquito.",
                    "Se firma convenio finiquito por virtud del cual el actor recibe la cantidad de $420,000.00 y otorga el más amplio finiquito. Se solicita el archivo del expediente como asunto totalmente concluido.")
            })
    };
}

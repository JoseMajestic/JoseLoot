param(
    [string]$ProjectRoot = (Get-Location).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Mappings
$sets = @{
    "Aprendiz" = "Tiro"
    "Arcano" = "Augur"
    "Cazador" = "Explorator"
    "Conquistador" = "Legionarius"
    "Heroe" = "Roma"
    "Fantasma" = "Speculator"
    "Titan" = "Colossus"
    "Serafin" = "Pontifex"
    "Abismo" = "Infernum"
    "Apocalipsis" = "Ultima Roma"
}

$levelSuffixes = @{
    "Tiro" = @("Rudimentum", "Veteranum", "Imperium")
    "Augur" = @("Omen", "Ritus", "Oraculum")
    "Explorator" = @("Peregrinus", "Venator", "Praedator")
    "Legionarius" = @("Cohors", "Centuria", "Legio")
    "Roma" = @("Virtus", "Honor", "Gloria")
    "Speculator" = @("Umbra", "Silentium", "Occultus")
    "Colossus" = @("Moles", "Titanus", "Colossus")
    "Pontifex" = @("Sacrum", "Divinus", "Sanctus")
    "Infernum" = @("Limbus", "Inferus", "Abyssus")
    "Ultima Roma" = @("Exitium", "Cataclysmus", "Aeternitas")
}

$itemTypes = @{
    "Arma" = "Arma"
    "Armadura" = "Armadura"
    "Casco" = "Casco"
    "Botas" = "Botas"
    "Guantes" = "Guantes"
    "Cinturon" = "Cinturón"
    "Collar" = "Collar"
    "Anillo" = "Anillo"
    "Escudo" = "Escudo"
    "Montura" = "Montura"
}

# Descriptions dictionary
$descriptions = @{

    # APRENDIZ (TIRO)
    "Arma de Tiro Rudimentum" = "Arma basica de entrenamiento, disenada para infligir dano simple durante el aprendizaje inicial."
    "Arma de Tiro Veteranum" = "Arma mejorada de instruccion, ofrece mayor fiabilidad tras un entrenamiento constante y disciplinado."
    "Arma de Tiro Imperium" = "Arma avanzada de formacion, refleja dominio tecnico adquirido mediante rigor militar prolongado."
    "Armadura de Tiro Rudimentum" = "Proteccion ligera de iniciacion, pensada para reducir dano durante las primeras batallas."
    "Armadura de Tiro Veteranum" = "Armadura reforzada de aprendizaje, mejora la supervivencia tras experiencia basica en combate."
    "Armadura de Tiro Imperium" = "Armadura completa de formacion, simboliza control defensivo logrado mediante practica continua."
    "Casco de Tiro Rudimentum" = "Casco sencillo que protege la cabeza durante entrenamientos y enfrentamientos menores."
    "Casco de Tiro Veteranum" = "Casco reforzado que incrementa resistencia tras superar las fases basicas de instruccion."
    "Casco de Tiro Imperium" = "Casco avanzado que refleja disciplina y control adquiridos en formacion militar completa."
    "Botas de Tiro Rudimentum" = "Calzado basico que proporciona estabilidad minima durante desplazamientos de entrenamiento."
    "Botas de Tiro Veteranum" = "Botas mejoradas que aumentan firmeza y control tras experiencia prolongada en combate."
    "Botas de Tiro Imperium" = "Botas de formacion avanzada que garantizan movilidad segura en situaciones exigentes."
    "Guantes de Tiro Rudimentum" = "Guantes simples que facilitan el manejo basico de armas durante la instruccion."
    "Guantes de Tiro Veteranum" = "Guantes reforzados que mejoran precision manual tras entrenamiento constante."
    "Guantes de Tiro Imperium" = "Guantes avanzados que reflejan control tecnico adquirido mediante disciplina prolongada."
    "Cinturón de Tiro Rudimentum" = "Cinturon funcional que mantiene el equipo basico organizado durante el aprendizaje."
    "Cinturón de Tiro Veteranum" = "Cinturon reforzado que mejora preparacion y equilibrio tras experiencia acumulada."
    "Cinturón de Tiro Imperium" = "Cinturon avanzado que simboliza orden y control propios de una formacion completa."
    "Collar de Tiro Rudimentum" = "Amuleto sencillo que acompana al aprendiz durante sus primeras pruebas."
    "Collar de Tiro Veteranum" = "Collar de instruccion que refuerza confianza tras superar entrenamientos iniciales."
    "Collar de Tiro Imperium" = "Collar representativo del dominio alcanzado en el proceso formativo completo."
    "Anillo de Tiro Rudimentum" = "Anillo simple que simboliza el inicio del camino marcial del portador."
    "Anillo de Tiro Veteranum" = "Anillo reforzado que refleja progreso y constancia en la formacion militar."
    "Anillo de Tiro Imperium" = "Anillo avanzado que representa control y madurez alcanzados tras entrenamiento completo."
    "Escudo de Tiro Rudimentum" = "Escudo basico que ensena defensa elemental durante las primeras batallas."
    "Escudo de Tiro Veteranum" = "Escudo reforzado que mejora la postura defensiva tras experiencia practica."
    "Escudo de Tiro Imperium" = "Escudo avanzado que demuestra dominio defensivo adquirido mediante formacion rigurosa."
    "Montura de Tiro Rudimentum" = "Montura basica entrenada para acompanar desplazamientos iniciales del aprendiz."
    "Montura de Tiro Veteranum" = "Montura disciplinada que mejora el rendimiento tras entrenamiento conjunto prolongado."
    "Montura de Tiro Imperium" = "Montura experta que refleja control total y sincronia logrados con formacion completa."

    # ARCAN0 (AUGUR)
    "Arma de Augur Omen" = "Arma ritual sencilla, canaliza presagios iniciales para apoyar ataques basicos del combatiente."
    "Arma de Augur Ritus" = "Arma ceremonial reforzada, potencia el dano mediante rituales magicos controlados."
    "Arma de Augur Oraculum" = "Arma consagrada que manifiesta la voluntad profetica en cada golpe ejecutado."
    "Armadura de Augur Omen" = "Vestimenta ritual ligera que ofrece proteccion basica bajo signos favorables."
    "Armadura de Augur Ritus" = "Armadura ceremonial que refuerza la defensa mediante rituales magicos establecidos."
    "Armadura de Augur Oraculum" = "Armadura sagrada que protege al portador bajo la guia directa del oraculo."
    "Casco de Augur Omen" = "Tocado ritual que protege la mente mientras se interpretan presagios iniciales."
    "Casco de Augur Ritus" = "Casco ceremonial que mejora la concentracion durante rituales magicos prolongados."
    "Casco de Augur Oraculum" = "Casco consagrado que canaliza visiones profeticas con claridad absoluta."
    "Botas de Augur Omen" = "Calzado ritual que permite desplazarse con estabilidad durante practicas magicas basicas."
    "Botas de Augur Ritus" = "Botas ceremoniales que mejoran el control corporal durante rituales complejos."
    "Botas de Augur Oraculum" = "Botas sagradas que aseguran paso firme guiado por visiones profeticas."
    "Guantes de Augur Omen" = "Guantes rituales que facilitan la manipulacion inicial de energias magicas."
    "Guantes de Augur Ritus" = "Guantes ceremoniales que mejoran el control preciso durante invocaciones rituales."
    "Guantes de Augur Oraculum" = "Guantes consagrados que canalizan poder magico con absoluta precision profetica."
    "Cinturón de Augur Omen" = "Cinturon ritual que organiza componentes basicos usados en practicas magicas iniciales."
    "Cinturón de Augur Ritus" = "Cinturon ceremonial que mejora la preparacion durante rituales formales."
    "Cinturón de Augur Oraculum" = "Cinturon sagrado que sostiene reliquias vinculadas al poder del oraculo."
    "Collar de Augur Omen" = "Amuleto simple que acompana al augur durante lecturas iniciales de presagios."
    "Collar de Augur Ritus" = "Collar ritual que refuerza la conexion magica durante ceremonias establecidas."
    "Collar de Augur Oraculum" = "Collar consagrado que amplifica la comunion directa con fuerzas profeticas."
    "Anillo de Augur Omen" = "Anillo ritual que simboliza el inicio del camino mistico del augur."
    "Anillo de Augur Ritus" = "Anillo ceremonial que refleja dominio creciente sobre rituales magicos."
    "Anillo de Augur Oraculum" = "Anillo sagrado que encarna la autoridad profetica del portador."
    "Escudo de Augur Omen" = "Escudo ritual basico que ofrece proteccion simbolica durante practicas magicas."
    "Escudo de Augur Ritus" = "Escudo ceremonial que refuerza la defensa mediante runas rituales activas."
    "Escudo de Augur Oraculum" = "Escudo consagrado que protege bajo la vision anticipada del oraculo."
    "Montura de Augur Omen" = "Montura tranquila entrenada para acompanar desplazamientos rituales iniciales."
    "Montura de Augur Ritus" = "Montura disciplinada que facilita viajes durante ceremonias magicas prolongadas."
    "Montura de Augur Oraculum" = "Montura sagrada guiada por presagios que asegura desplazamientos seguros."

    # CAZADOR (EXPLORATOR)
    "Arma de Explorator Peregrinus" = "Arma ligera disenada para combates ocasionales durante exploraciones prolongadas en territorio desconocido."
    "Arma de Explorator Venator" = "Arma equilibrada que mejora la eficacia ofensiva durante cacerias y escaramuzas continuas."
    "Arma de Explorator Praedator" = "Arma especializada que maximiza el dano durante emboscadas rapidas y ataques decisivos."
    "Armadura de Explorator Peregrinus" = "Armadura flexible que ofrece proteccion basica sin limitar el movimiento durante largas travesias."
    "Armadura de Explorator Venator" = "Armadura reforzada que equilibra defensa y movilidad en misiones de caza avanzada."
    "Armadura de Explorator Praedator" = "Armadura optimizada para resistir impactos mientras mantiene agilidad en combates rapidos."
    "Casco de Explorator Peregrinus" = "Casco ligero que protege sin obstaculizar la vision durante exploraciones iniciales."
    "Casco de Explorator Venator" = "Casco reforzado que mejora la proteccion manteniendo atencion constante al entorno."
    "Casco de Explorator Praedator" = "Casco tactico que ofrece proteccion solida durante persecuciones y ataques sorpresa."
    "Botas de Explorator Peregrinus" = "Botas resistentes que facilitan largas caminatas sobre terrenos irregulares."
    "Botas de Explorator Venator" = "Botas reforzadas que mejoran la traccion durante persecuciones prolongadas."
    "Botas de Explorator Praedator" = "Botas especializadas que permiten movimientos rapidos y silenciosos en combate."
    "Guantes de Explorator Peregrinus" = "Guantes funcionales que mejoran el agarre durante exploraciones basicas."
    "Guantes de Explorator Venator" = "Guantes reforzados que aumentan el control manual en cacerias intensas."
    "Guantes de Explorator Praedator" = "Guantes tacticos que optimizan precision y velocidad en ataques rapidos."
    "Cinturón de Explorator Peregrinus" = "Cinturon practico que organiza herramientas esenciales durante largos desplazamientos."
    "Cinturón de Explorator Venator" = "Cinturon reforzado que mejora la preparacion en misiones de caza avanzada."
    "Cinturón de Explorator Praedator" = "Cinturon tactico que permite acceso rapido a equipo durante emboscadas."
    "Collar de Explorator Peregrinus" = "Amuleto sencillo que acompana al explorador durante travesias prolongadas."
    "Collar de Explorator Venator" = "Collar funcional que refuerza la concentracion durante rastreos intensivos."
    "Collar de Explorator Praedator" = "Collar distintivo que simboliza experiencia en cacerias letales."
    "Anillo de Explorator Peregrinus" = "Anillo simple que representa el inicio del camino del explorador."
    "Anillo de Explorator Venator" = "Anillo reforzado que refleja habilidad creciente en rastreo y combate."
    "Anillo de Explorator Praedator" = "Anillo tactico que simboliza dominio total de la caza."
    "Escudo de Explorator Peregrinus" = "Escudo ligero que ofrece proteccion basica sin comprometer la movilidad."
    "Escudo de Explorator Venator" = "Escudo equilibrado que permite defenderse eficazmente durante combates prolongados."
    "Escudo de Explorator Praedator" = "Escudo resistente que protege durante enfrentamientos rapidos y agresivos."
    "Montura de Explorator Peregrinus" = "Montura resistente entrenada para recorrer largas distancias sin descanso."
    "Montura de Explorator Venator" = "Montura agil que facilita persecuciones sostenidas en terrenos variados."
    "Montura de Explorator Praedator" = "Montura veloz entrenada para ataques rapidos y retiradas inmediatas."

    # CONQUISTADOR (LEGIONARIUS)
    "Arma de Legionarius Cohors" = "Arma reglamentaria disenada para combatir en formacion cerrada junto a la cohorte."
    "Arma de Legionarius Centuria" = "Arma militar mejorada que incrementa la eficacia ofensiva en unidades organizadas."
    "Arma de Legionarius Legio" = "Arma de elite forjada para guerras a gran escala bajo mando legionarios."
    "Armadura de Legionarius Cohors" = "Armadura estandar que protege eficazmente durante combates disciplinados en formacion."
    "Armadura de Legionarius Centuria" = "Armadura reforzada que mejora la resistencia en enfrentamientos prolongados."
    "Armadura de Legionarius Legio" = "Armadura pesada disenada para resistir campanas militares intensivas."
    "Casco de Legionarius Cohors" = "Casco militar basico que protege la cabeza durante maniobras reglamentarias."
    "Casco de Legionarius Centuria" = "Casco reforzado que incrementa la supervivencia en batallas organizadas."
    "Casco de Legionarius Legio" = "Casco de guerra avanzado usado en campanas de conquista extensas."
    "Botas de Legionarius Cohors" = "Botas militares resistentes disenadas para marchas prolongadas en formacion."
    "Botas de Legionarius Centuria" = "Botas reforzadas que mantienen estabilidad durante combates intensos."
    "Botas de Legionarius Legio" = "Botas de campana preparadas para resistir terrenos hostiles prolongados."
    "Guantes de Legionarius Cohors" = "Guantes militares que facilitan el manejo seguro de armas reglamentarias."
    "Guantes de Legionarius Centuria" = "Guantes reforzados que mejoran control y resistencia durante el combate."
    "Guantes de Legionarius Legio" = "Guantes de guerra disenados para campanas largas y enfrentamientos constantes."
    "Cinturón de Legionarius Cohors" = "Cinturon militar que organiza equipo esencial durante el combate disciplinado."
    "Cinturón de Legionarius Centuria" = "Cinturon reforzado que mejora preparacion y eficiencia logistica en batalla."
    "Cinturón de Legionarius Legio" = "Cinturon de campana que simboliza orden y jerarquia legionaria."
    "Collar de Legionarius Cohors" = "Distintivo sencillo que identifica pertenencia a una cohorte romana."
    "Collar de Legionarius Centuria" = "Collar militar que simboliza servicio activo en unidades avanzadas."
    "Collar de Legionarius Legio" = "Emblema de honor que representa participacion en campanas imperiales."
    "Anillo de Legionarius Cohors" = "Anillo militar basico que marca el juramento de servicio legionarios."
    "Anillo de Legionarius Centuria" = "Anillo reforzado que simboliza rango y experiencia en combate."
    "Anillo de Legionarius Legio" = "Anillo de guerra que representa lealtad absoluta a la legion."
    "Escudo de Legionarius Cohors" = "Escudo reglamentario disenado para defensa colectiva en formacion cerrada."
    "Escudo de Legionarius Centuria" = "Escudo reforzado que mejora la resistencia durante choques prolongados."
    "Escudo de Legionarius Legio" = "Escudo pesado capaz de soportar asedios y batallas intensivas."
    "Montura de Legionarius Cohors" = "Montura entrenada para desplazamientos militares controlados y seguros."
    "Montura de Legionarius Centuria" = "Montura disciplinada que mejora movilidad tactica en campanas activas."
    "Montura de Legionarius Legio" = "Montura de guerra preparada para operaciones imperiales prolongadas."

    # HÉROE (ROMA)
    "Arma de Roma Virtus" = "Arma honorable que refleja valentia personal demostrada en multiples batallas decisivas."
    "Arma de Roma Honor" = "Arma distinguida que simboliza rectitud y compromiso con los valores de Roma."
    "Arma de Roma Gloria" = "Arma legendaria que encarna fama y reconocimiento obtenidos mediante hazanas memorables."
    "Armadura de Roma Virtus" = "Armadura noble que protege al portador reconocido por su coraje probado."
    "Armadura de Roma Honor" = "Armadura ceremonial que refleja integridad y respeto ganado en combate."
    "Armadura de Roma Gloria" = "Armadura ilustre que simboliza prestigio alcanzado tras grandes victorias."
    "Casco de Roma Virtus" = "Casco distinguido que identifica a guerreros valientes dentro del ejercito romano."
    "Casco de Roma Honor" = "Casco ceremonial que representa disciplina y respeto ganados ante Roma."
    "Casco de Roma Gloria" = "Casco emblematico que proclama fama militar reconocida publicamente."
    "Botas de Roma Virtus" = "Botas robustas usadas por heroes que han demostrado valor en campana."
    "Botas de Roma Honor" = "Botas ceremoniales que acompanan a guerreros respetados por su conducta ejemplar."
    "Botas de Roma Gloria" = "Botas prestigiosas asociadas a heroes celebrados tras grandes gestas."
    "Guantes de Roma Virtus" = "Guantes refinados que mejoran el control del arma en manos valientes."
    "Guantes de Roma Honor" = "Guantes ceremoniales que reflejan disciplina y respeto dentro del ejercito."
    "Guantes de Roma Gloria" = "Guantes ilustres usados por combatientes celebrados por su fama."
    "Cinturón de Roma Virtus" = "Cinturon distintivo que simboliza compromiso personal con el deber romano."
    "Cinturón de Roma Honor" = "Cinturon ceremonial que representa reputacion intachable ganada en servicio."
    "Cinturón de Roma Gloria" = "Cinturon emblematico asociado a figuras admiradas por toda Roma."
    "Collar de Roma Virtus" = "Distintivo noble que reconoce valor demostrado en el campo de batalla."
    "Collar de Roma Honor" = "Emblema ceremonial que identifica a guerreros respetados por su conducta."
    "Collar de Roma Gloria" = "Collar prestigioso que proclama fama obtenida mediante victorias notables."
    "Anillo de Roma Virtus" = "Anillo honorifico que simboliza valentia reconocida oficialmente por Roma."
    "Anillo de Roma Honor" = "Anillo ceremonial que representa integridad y lealtad demostradas en servicio."
    "Anillo de Roma Gloria" = "Anillo ilustre que marca al portador como heroe celebrado."
    "Escudo de Roma Virtus" = "Escudo honorable usado por defensores valientes en batallas decisivas."
    "Escudo de Roma Honor" = "Escudo ceremonial que refleja disciplina y respeto dentro de las filas."
    "Escudo de Roma Gloria" = "Escudo emblematico que proclama gloria militar ante aliados y enemigos."
    "Montura de Roma Virtus" = "Montura noble asignada a heroes reconocidos por su valentia."
    "Montura de Roma Honor" = "Montura ceremonial que acompana a figuras respetadas dentro del ejercito."
    "Montura de Roma Gloria" = "Montura prestigiosa reservada a heroes celebrados por toda Roma."

    # FANTASMA (SPECULATOR)
    "Arma de Speculator Umbra" = "Arma discreta disenada para ataques rapidos sin revelar la posicion del portador."
    "Arma de Speculator Silentium" = "Arma optimizada para eliminar objetivos sin alertar a fuerzas enemigas cercanas."
    "Arma de Speculator Occultus" = "Arma especializada para operaciones secretas donde la deteccion resulta fatal."
    "Armadura de Speculator Umbra" = "Armadura ligera que reduce ruido y visibilidad durante misiones encubiertas."
    "Armadura de Speculator Silentium" = "Armadura reforzada que mantiene sigilo incluso en enfrentamientos prolongados."
    "Armadura de Speculator Occultus" = "Armadura avanzada disenada para desaparecer entre sombras y confusion."
    "Casco de Speculator Umbra" = "Casco discreto que protege sin interferir con percepcion ni sigilo."
    "Casco de Speculator Silentium" = "Casco tactico que atenua sonidos y mejora la concentracion operativa."
    "Casco de Speculator Occultus" = "Casco especializado que oculta presencia y refuerza la conciencia situacional."
    "Botas de Speculator Umbra" = "Botas silenciosas que permiten desplazamientos rapidos sin dejar rastro."
    "Botas de Speculator Silentium" = "Botas reforzadas que amortiguan pasos durante infiltraciones prolongadas."
    "Botas de Speculator Occultus" = "Botas avanzadas disenadas para movimientos invisibles en cualquier terreno."
    "Guantes de Speculator Umbra" = "Guantes ligeros que mejoran precision manual durante operaciones sigilosas."
    "Guantes de Speculator Silentium" = "Guantes tacticos que permiten manipulacion precisa sin producir ruido."
    "Guantes de Speculator Occultus" = "Guantes especializados que optimizan control absoluto en misiones encubiertas."
    "Cinturón de Speculator Umbra" = "Cinturon discreto que organiza herramientas esenciales para infiltracion basica."
    "Cinturón de Speculator Silentium" = "Cinturon tactico que facilita acceso rapido a equipo sin delatar presencia."
    "Cinturón de Speculator Occultus" = "Cinturon avanzado disenado para operaciones prolongadas en secreto absoluto."
    "Collar de Speculator Umbra" = "Amuleto discreto que simboliza pertenencia a redes de informacion secreta."
    "Collar de Speculator Silentium" = "Distintivo oculto usado por agentes dedicados a misiones silenciosas."
    "Collar de Speculator Occultus" = "Emblema reservado que identifica a maestros del espionaje romano."
    "Anillo de Speculator Umbra" = "Anillo sencillo que marca iniciacion en labores de inteligencia encubierta."
    "Anillo de Speculator Silentium" = "Anillo tactico que simboliza experiencia en operaciones secretas exitosas."
    "Anillo de Speculator Occultus" = "Anillo reservado que representa autoridad en redes de espionaje imperiales."
    "Escudo de Speculator Umbra" = "Escudo ligero que ofrece proteccion minima sin comprometer movilidad ni sigilo."
    "Escudo de Speculator Silentium" = "Escudo tactico que permite defensa rapida durante retiradas silenciosas."
    "Escudo de Speculator Occultus" = "Escudo especializado disenado para proteger sin revelar la posicion."
    "Montura de Speculator Umbra" = "Montura entrenada para desplazamientos discretos en misiones de reconocimiento."
    "Montura de Speculator Silentium" = "Montura silenciosa preparada para infiltraciones prolongadas sin deteccion."
    "Montura de Speculator Occultus" = "Montura experta utilizada en operaciones secretas de maxima prioridad."

    # TITÁN (COLOSSUS)
    "Arma de Colossus Moles" = "Arma pesada disenada para causar gran impacto mediante fuerza bruta sostenida."
    "Arma de Colossus Titanus" = "Arma masiva que incrementa el dano aprovechando potencia y resistencia excepcionales."
    "Arma de Colossus Colossus" = "Arma colosal capaz de devastar enemigos mediante golpes lentos pero aplastantes."
    "Armadura de Colossus Moles" = "Armadura gruesa que prioriza proteccion frente a movilidad en combate directo."
    "Armadura de Colossus Titanus" = "Armadura reforzada que permite resistir castigos prolongados sin ceder terreno."
    "Armadura de Colossus Colossus" = "Armadura colosal disenada para soportar asaltos continuos en batallas frontales."
    "Casco de Colossus Moles" = "Casco pesado que protege eficazmente la cabeza durante enfrentamientos directos."
    "Casco de Colossus Titanus" = "Casco reforzado que incrementa resistencia ante impactos constantes."
    "Casco de Colossus Colossus" = "Casco colosal que simboliza invulnerabilidad frente a ataques enemigos."
    "Botas de Colossus Moles" = "Botas robustas que aseguran estabilidad durante combates de fuerza bruta."
    "Botas de Colossus Titanus" = "Botas reforzadas que permiten avanzar firmemente bajo presion constante."
    "Botas de Colossus Colossus" = "Botas colosales que mantienen al portador imparable en el campo de batalla."
    "Guantes de Colossus Moles" = "Guantes pesados que mejoran agarre y control de armas masivas."
    "Guantes de Colossus Titanus" = "Guantes reforzados que soportan tensiones extremas durante el combate."
    "Guantes de Colossus Colossus" = "Guantes colosales disenados para manejar fuerza devastadora sin perdida de control."
    "Cinturón de Colossus Moles" = "Cinturon resistente que sostiene equipo pesado durante enfrentamientos prolongados."
    "Cinturón de Colossus Titanus" = "Cinturon reforzado que mejora equilibrio bajo cargas extremas."
    "Cinturón de Colossus Colossus" = "Cinturon colosal que simboliza dominio absoluto de la fuerza."
    "Collar de Colossus Moles" = "Amuleto robusto que representa fortaleza fisica y resistencia constante."
    "Collar de Colossus Titanus" = "Collar pesado que simboliza poder acumulado mediante combate persistente."
    "Collar de Colossus Colossus" = "Collar colosal que encarna la supremacia fisica del portador."
    "Anillo de Colossus Moles" = "Anillo solido que marca dedicacion al combate de fuerza directa."
    "Anillo de Colossus Titanus" = "Anillo reforzado que refleja resistencia y poder ganados con esfuerzo."
    "Anillo de Colossus Colossus" = "Anillo colosal que simboliza dominio total de la fuerza bruta."
    "Escudo de Colossus Moles" = "Escudo pesado que absorbe impactos directos sin ceder posicion."
    "Escudo de Colossus Titanus" = "Escudo reforzado capaz de soportar ataques continuos en primera linea."
    "Escudo de Colossus Colossus" = "Escudo colosal disenado para resistir asaltos masivos sin romper formacion."
    "Montura de Colossus Moles" = "Montura poderosa entrenada para soportar el peso del guerrero y su equipo."
    "Montura de Colossus Titanus" = "Montura resistente preparada para cargas frontales prolongadas."
    "Montura de Colossus Colossus" = "Montura colosal utilizada por guerreros imponentes en batallas decisivas."

    # SERAFÍN (PONTIFEX)
    "Arma de Pontifex Sacrum" = "Arma consagrada utilizada en combates rituales bajo bendicion de los cultos oficiales."
    "Arma de Pontifex Divinus" = "Arma sagrada que canaliza poder religioso mediante ceremonias formales."
    "Arma de Pontifex Sanctus" = "Arma santificada que ejecuta la voluntad divina con autoridad absoluta."
    "Armadura de Pontifex Sacrum" = "Vestimenta ceremonial que ofrece proteccion basica durante rituales y enfrentamientos sagrados."
    "Armadura de Pontifex Divinus" = "Armadura bendecida que refuerza la defensa mediante proteccion religiosa activa."
    "Armadura de Pontifex Sanctus" = "Armadura sagrada que envuelve al portador bajo amparo divino constante."
    "Casco de Pontifex Sacrum" = "Tocado ceremonial que protege la mente durante actos y combates rituales."
    "Casco de Pontifex Divinus" = "Casco bendecido que mejora concentracion y resistencia espiritual."
    "Casco de Pontifex Sanctus" = "Casco sagrado que simboliza autoridad religiosa incuestionable en batalla."
    "Botas de Pontifex Sacrum" = "Calzado ritual que permite desplazarse con solemnidad durante ceremonias sagradas."
    "Botas de Pontifex Divinus" = "Botas bendecidas que otorgan firmeza bajo guia divina."
    "Botas de Pontifex Sanctus" = "Botas sagradas que aseguran paso firme en nombre de los dioses."
    "Guantes de Pontifex Sacrum" = "Guantes rituales que facilitan la ejecucion precisa de ceremonias sagradas."
    "Guantes de Pontifex Divinus" = "Guantes bendecidos que canalizan energia divina con control absoluto."
    "Guantes de Pontifex Sanctus" = "Guantes sagrados que manifiestan autoridad divina en cada accion."
    "Cinturón de Pontifex Sacrum" = "Cinturon ceremonial que sostiene reliquias usadas en rituales basicos."
    "Cinturón de Pontifex Divinus" = "Cinturon bendecido que mejora preparacion durante ceremonias prolongadas."
    "Cinturón de Pontifex Sanctus" = "Cinturon sagrado que simboliza dominio completo del rito divino."
    "Collar de Pontifex Sacrum" = "Amuleto ceremonial que refuerza vinculo inicial con los cultos oficiales."
    "Collar de Pontifex Divinus" = "Collar bendecido que amplifica la comunion con fuerzas divinas."
    "Collar de Pontifex Sanctus" = "Collar sagrado que encarna la voz de los dioses."
    "Anillo de Pontifex Sacrum" = "Anillo ritual que simboliza inicio del servicio religioso romano."
    "Anillo de Pontifex Divinus" = "Anillo bendecido que representa autoridad espiritual creciente."
    "Anillo de Pontifex Sanctus" = "Anillo sagrado que confirma supremacia religiosa absoluta."
    "Escudo de Pontifex Sacrum" = "Escudo ceremonial que ofrece proteccion simbolica durante ritos sagrados."
    "Escudo de Pontifex Divinus" = "Escudo bendecido que protege bajo amparo divino constante."
    "Escudo de Pontifex Sanctus" = "Escudo sagrado que manifiesta proteccion divina incuestionable."
    "Montura de Pontifex Sacrum" = "Montura entrenada para procesiones y desplazamientos rituales solemnes."
    "Montura de Pontifex Divinus" = "Montura bendecida que acompana ceremonias de gran importancia religiosa."
    "Montura de Pontifex Sanctus" = "Montura sagrada reservada a autoridades supremas del culto romano."

    # ABISMO (INFERNUM)
    "Arma de Infernum Limbus" = "Arma corrupta inicial que inflige dano mientras consume lentamente la esencia del combate."
    "Arma de Infernum Inferus" = "Arma oscura que incrementa poder ofensivo mediante energias prohibidas."
    "Arma de Infernum Abyssus" = "Arma abismal que desata destruccion sostenida alimentada por corrupcion extrema."
    "Armadura de Infernum Limbus" = "Armadura corrupta que ofrece proteccion basica a costa de estabilidad espiritual."
    "Armadura de Infernum Inferus" = "Armadura oscura que refuerza defensa mediante energias infernales activas."
    "Armadura de Infernum Abyssus" = "Armadura abismal que envuelve al portador en proteccion corrupta constante."
    "Casco de Infernum Limbus" = "Casco oscuro que protege la mente mientras expone al portador a susurros prohibidos."
    "Casco de Infernum Inferus" = "Casco infernal que incrementa resistencia mental bajo influencia corrupta."
    "Casco de Infernum Abyssus" = "Casco abismal que anula el temor mediante dominio total de la corrupcion."
    "Botas de Infernum Limbus" = "Botas corruptas que permiten avanzar sin sentir el peso del terreno."
    "Botas de Infernum Inferus" = "Botas infernales que mejoran estabilidad durante combates prolongados y caoticos."
    "Botas de Infernum Abyssus" = "Botas abismales que sostienen al portador en entornos hostiles extremos."
    "Guantes de Infernum Limbus" = "Guantes oscuros que mejoran el control ofensivo mediante energia corrupta inicial."
    "Guantes de Infernum Inferus" = "Guantes infernales que amplifican fuerza a costa de desgaste espiritual."
    "Guantes de Infernum Abyssus" = "Guantes abismales que canalizan poder destructivo sin restricciones."
    "Cinturón de Infernum Limbus" = "Cinturon corrupto que organiza reliquias prohibidas usadas en combate."
    "Cinturón de Infernum Inferus" = "Cinturon infernal que sostiene energia oscura durante enfrentamientos prolongados."
    "Cinturón de Infernum Abyssus" = "Cinturon abismal que mantiene estable el flujo de corrupcion."
    "Collar de Infernum Limbus" = "Amuleto oscuro que simboliza el primer vinculo con fuerzas prohibidas."
    "Collar de Infernum Inferus" = "Collar infernal que refuerza la conexion con energias corruptas."
    "Collar de Infernum Abyssus" = "Collar abismal que encarna sumision total a la corrupcion."
    "Anillo de Infernum Limbus" = "Anillo oscuro que marca el inicio del pacto prohibido."
    "Anillo de Infernum Inferus" = "Anillo infernal que simboliza poder obtenido mediante corrupcion consciente."
    "Anillo de Infernum Abyssus" = "Anillo abismal que representa dominio absoluto de fuerzas corruptas."
    "Escudo de Infernum Limbus" = "Escudo corrupto que absorbe impactos mientras filtra energia oscura."
    "Escudo de Infernum Inferus" = "Escudo infernal que protege mediante resistencia nacida del caos."
    "Escudo de Infernum Abyssus" = "Escudo abismal que bloquea ataques con corrupcion solidificada."
    "Montura de Infernum Limbus" = "Montura corrompida entrenada para resistir entornos inestables y hostiles."
    "Montura de Infernum Inferus" = "Montura infernal que soporta largas marchas bajo influencia corrupta."
    "Montura de Infernum Abyssus" = "Montura abismal ligada al portador por corrupcion irreversible."

    # APOCALIPSIS (ULTIMA ROMA)
    "Arma de Ultima Roma Exitium" = "Arma final disenada para infligir destruccion masiva en los ultimos conflictos del Imperio."
    "Arma de Ultima Roma Cataclysmus" = "Arma devastadora que arrasa enemigos durante el colapso total del orden establecido."
    "Arma de Ultima Roma Aeternitas" = "Arma absoluta que simboliza el legado eterno de Roma mas alla del fin."
    "Armadura de Ultima Roma Exitium" = "Armadura final que protege durante las ultimas batallas del Imperio agonizante."
    "Armadura de Ultima Roma Cataclysmus" = "Armadura devastada pero resistente, disenada para sobrevivir al colapso del mundo conocido."
    "Armadura de Ultima Roma Aeternitas" = "Armadura eterna que trasciende la destruccion y preserva el legado imperial."
    "Casco de Ultima Roma Exitium" = "Casco final usado en los ultimos combates que sellan el destino de Roma."
    "Casco de Ultima Roma Cataclysmus" = "Casco reforzado que resiste el caos absoluto de la caida imperial."
    "Casco de Ultima Roma Aeternitas" = "Casco eterno que encarna la memoria imperecedera del Imperio."
    "Botas de Ultima Roma Exitium" = "Botas de campana usadas durante las marchas finales hacia la destruccion."
    "Botas de Ultima Roma Cataclysmus" = "Botas resistentes que soportan terrenos arrasados por el colapso total."
    "Botas de Ultima Roma Aeternitas" = "Botas eternas que sostienen al portador mas alla del fin del Imperio."
    "Guantes de Ultima Roma Exitium" = "Guantes de guerra empleados en los ultimos enfrentamientos decisivos."
    "Guantes de Ultima Roma Cataclysmus" = "Guantes reforzados que permiten combatir en medio del caos absoluto."
    "Guantes de Ultima Roma Aeternitas" = "Guantes eternos que simbolizan dominio incluso tras la caida de Roma."
    "Cinturón de Ultima Roma Exitium" = "Cinturon final que sostiene equipo durante las ultimas defensas imperiales."
    "Cinturón de Ultima Roma Cataclysmus" = "Cinturon reforzado que mantiene orden en medio del colapso total."
    "Cinturón de Ultima Roma Aeternitas" = "Cinturon eterno que representa la continuidad del Imperio mas alla del fin."
    "Collar de Ultima Roma Exitium" = "Emblema final que simboliza lealtad en los ultimos dias de Roma."
    "Collar de Ultima Roma Cataclysmus" = "Collar devastado que representa fidelidad durante la ruina imperial."
    "Collar de Ultima Roma Aeternitas" = "Collar eterno que conserva la identidad romana tras la destruccion."
    "Anillo de Ultima Roma Exitium" = "Anillo final que marca el juramento en los ultimos momentos del Imperio."
    "Anillo de Ultima Roma Cataclysmus" = "Anillo reforzado que simboliza autoridad durante el colapso definitivo."
    "Anillo de Ultima Roma Aeternitas" = "Anillo eterno que representa el legado inmortal de Roma."
    "Escudo de Ultima Roma Exitium" = "Escudo final levantado en las ultimas defensas desesperadas del Imperio."
    "Escudo de Ultima Roma Cataclysmus" = "Escudo resistente que soporta el asalto final del mundo en ruinas."
    "Escudo de Ultima Roma Aeternitas" = "Escudo eterno que protege el legado imperial mas alla del tiempo."
    "Montura de Ultima Roma Exitium" = "Montura exhausta utilizada durante las ultimas campanas imperiales."
    "Montura de Ultima Roma Cataclysmus" = "Montura resistente que atraviesa paisajes destruidos por el colapso total."
    "Montura de Ultima Roma Aeternitas" = "Montura eterna que simboliza la marcha infinita del legado romano."
}

# Function to update YAML value
function Update-YamlValue {
    param(
        [string[]]$lines,
        [string]$field,
        [string]$value
    )
    $pattern = "^(\s*){0}:\s*" -f [regex]::Escape($field)
    for ($i = 0; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -match $pattern) {
            $indent = $matches[1]
            $lines[$i] = "${indent}${field}: $value"
            return $lines
        }
    }
    Write-Warning "Field '$field' not found in file"
    return $lines
}

# Find all .asset files in Assets/Items/
$itemAssets = Get-ChildItem -Path "$ProjectRoot\Assets\Items" -Filter "*.asset" -File

foreach ($asset in $itemAssets) {
    $fileName = $asset.Name
    # Parse fileName: e.g. "Aprendiz Anillo I.asset"
    if ($fileName -match "^(\w+) (\w+) ([IV]+)\.asset$") {
        $set = $matches[1]
        $itemType = $matches[2]
        $level = $matches[3]
        
        if ($sets.ContainsKey($set) -and $itemTypes.ContainsKey($itemType)) {
            $setKey = $sets[$set]
            $levelIndex = switch ($level) {
                "I" { 0 }
                "II" { 1 }
                "III" { 2 }
                default { continue }
            }
            $suffix = $levelSuffixes[$setKey][$levelIndex]
            $newItemName = "$($itemTypes[$itemType]) de $($setKey) $($suffix)"
            
            if ($descriptions.ContainsKey($newItemName)) {
                $newDescription = $descriptions[$newItemName]
                
                # Read file
                $lines = Get-Content -LiteralPath $asset.FullName
                
                # Update itemName
                $lines = Update-YamlValue -lines $lines -field "itemName" -value $newItemName
                
                # Update description
                $lines = Update-YamlValue -lines $lines -field "description" -value $newDescription
                
                # Write back
                Set-Content -LiteralPath $asset.FullName -Value $lines -Encoding UTF8
                
                Write-Host "Updated $fileName to $newItemName"
            } else {
                Write-Warning "No description found for $newItemName"
            }
        } else {
            Write-Warning "Unknown set or itemType in $fileName"
        }
    } else {
        Write-Warning "FileName format not recognized: $fileName"
    }
}

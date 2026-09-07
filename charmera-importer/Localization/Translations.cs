using System.Collections.Generic;

namespace charmera_importer.Localization;

// Single source of truth for every user-facing string. Keep Portuguese and English
// in sync — LocalizedStrings falls back to the raw key if a lookup misses, which is
// visible enough during development to catch a missed translation.
internal static class Translations
{
    private static readonly Dictionary<string, string> Portuguese = new()
    {
        ["Header_DeviceLabel"] = "DISPOSITIVO",
        ["Header_DevicePlaceholder"] = "Nenhum dispositivo",
        ["Header_RefreshTooltip"] = "Atualizar lista de dispositivos",
        ["Header_LanguageLabel"] = "IDIOMA",

        ["Sidebar_DestinationLabel"] = "PASTA DE DESTINO",
        ["Sidebar_NoDestinationSelected"] = "Nenhuma pasta selecionada",
        ["Sidebar_BrowseButton"] = "Escolher pasta...",
        ["Sidebar_OrganizeLabel"] = "ORGANIZAR POR",
        ["Sidebar_NamingLabel"] = "NOME DO ARQUIVO",
        ["Sidebar_KeepOriginalName"] = "Manter nome original como sufixo",
        ["Sidebar_DeleteAfterImport"] = "Apagar da câmera após importar",
        ["Sidebar_DeleteAfterImportWarning"] = "Remove permanentemente as fotos da câmera depois de confirmar a importação. Não pode ser desfeito.",
        ["Sidebar_ImportButton"] = "Importar fotos",

        ["Content_PhotosTitle"] = "Fotos",
        ["Content_PhotosCountFormat"] = "{0} foto(s)",
        ["Content_EmptyStateTitle"] = "Selecione um dispositivo para ver as fotos",
        ["Content_EmptyStateSubtitle"] = "Conecte a câmera via USB e escolha-a no topo da tela.",

        ["Detail_Title"] = "DETALHES EXIF",
        ["Detail_Make"] = "Marca",
        ["Detail_Model"] = "Modelo",
        ["Detail_Date"] = "Data",
        ["Detail_Dimensions"] = "Dimensões",
        ["Detail_NoExifNote"] = "Esta câmera não grava marca/modelo/data no EXIF.",
        ["Detail_AllTags"] = "TODAS AS TAGS",

        ["Org_YearMonth"] = "Ano/Mês (2026/03)",
        ["Org_YearMonthDay"] = "Ano/Mês/Dia (2026/03/15)",
        ["Org_ByCameraModel"] = "Por modelo de câmera",
        ["Org_Flat"] = "Pasta única (sem subpastas)",

        ["Status_Scanning"] = "Procurando fotos...",
        ["Status_PhotosFoundFormat"] = "{0} foto(s) encontrada(s)",
        ["Status_ImportingFormat"] = "Importando {0} ({1}/{2})",
        ["Status_ImportComplete"] = "Importação concluída",

        ["Import_AlreadyImported"] = "Já importada anteriormente",
        ["Import_AlreadyAtDestination"] = "Já existe no destino",
        ["Import_Success"] = "Importada",
        ["Import_ErrorFormat"] = "Erro: {0}",
        ["Import_DeleteFailedFormat"] = "falha ao apagar da câmera: {0}",

        ["StatusLabel_Imported"] = "Importada",
        ["StatusLabel_Duplicate"] = "Duplicada",
        ["StatusLabel_Error"] = "Erro",
        ["StatusLabel_Pending"] = "Pendente",

        ["FolderPicker_Title"] = "Escolher pasta de destino",
    };

    private static readonly Dictionary<string, string> English = new()
    {
        ["Header_DeviceLabel"] = "DEVICE",
        ["Header_DevicePlaceholder"] = "No device",
        ["Header_RefreshTooltip"] = "Refresh device list",
        ["Header_LanguageLabel"] = "LANGUAGE",

        ["Sidebar_DestinationLabel"] = "DESTINATION FOLDER",
        ["Sidebar_NoDestinationSelected"] = "No folder selected",
        ["Sidebar_BrowseButton"] = "Choose folder...",
        ["Sidebar_OrganizeLabel"] = "ORGANIZE BY",
        ["Sidebar_NamingLabel"] = "FILE NAME",
        ["Sidebar_KeepOriginalName"] = "Keep original filename as suffix",
        ["Sidebar_DeleteAfterImport"] = "Delete from camera after importing",
        ["Sidebar_DeleteAfterImportWarning"] = "Permanently removes photos from the camera once the import is confirmed. This cannot be undone.",
        ["Sidebar_ImportButton"] = "Import photos",

        ["Content_PhotosTitle"] = "Photos",
        ["Content_PhotosCountFormat"] = "{0} photo(s)",
        ["Content_EmptyStateTitle"] = "Select a device to see its photos",
        ["Content_EmptyStateSubtitle"] = "Connect the camera via USB and choose it at the top of the screen.",

        ["Detail_Title"] = "EXIF DETAILS",
        ["Detail_Make"] = "Make",
        ["Detail_Model"] = "Model",
        ["Detail_Date"] = "Date",
        ["Detail_Dimensions"] = "Dimensions",
        ["Detail_NoExifNote"] = "This camera doesn't write make/model/date to EXIF.",
        ["Detail_AllTags"] = "ALL TAGS",

        ["Org_YearMonth"] = "Year/Month (2026/03)",
        ["Org_YearMonthDay"] = "Year/Month/Day (2026/03/15)",
        ["Org_ByCameraModel"] = "By camera model",
        ["Org_Flat"] = "Single folder (no subfolders)",

        ["Status_Scanning"] = "Looking for photos...",
        ["Status_PhotosFoundFormat"] = "{0} photo(s) found",
        ["Status_ImportingFormat"] = "Importing {0} ({1}/{2})",
        ["Status_ImportComplete"] = "Import complete",

        ["Import_AlreadyImported"] = "Already imported before",
        ["Import_AlreadyAtDestination"] = "Already exists at destination",
        ["Import_Success"] = "Imported",
        ["Import_ErrorFormat"] = "Error: {0}",
        ["Import_DeleteFailedFormat"] = "failed to delete from camera: {0}",

        ["StatusLabel_Imported"] = "Imported",
        ["StatusLabel_Duplicate"] = "Duplicate",
        ["StatusLabel_Error"] = "Error",
        ["StatusLabel_Pending"] = "Pending",

        ["FolderPicker_Title"] = "Choose destination folder",
    };

    public static IReadOnlyDictionary<string, string> Get(string languageCode) => languageCode switch
    {
        "pt" => Portuguese,
        _ => English,
    };
}

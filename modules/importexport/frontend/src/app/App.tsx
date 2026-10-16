import { useEffect, useMemo } from 'react'
import {
  Navigate,
  Route,
  Routes,
  useNavigate,
  useParams,
} from 'react-router-dom'
import {
  AdminLayout,
  DEFAULT_ADMIN_MAIN_NAV,
  type AdminNavItem,
} from '@platform/core'
import { FileSpreadsheet } from 'lucide-react'
import {
  IMPORT_EXPORT_ROUTES,
  TemplateListPage,
  TemplateFormPage,
  TemplateMetadataPage,
  TemplateConfigurationPage,
  ExcelImportPage,
  configureImportExportNavigate,
  getImportExportTemplateListPath,
} from '@platform/importexport'

function NavigateBridge() {
  const navigate = useNavigate()
  useEffect(() => {
    configureImportExportNavigate((to) => navigate(to))
  }, [navigate])
  return null
}

function TemplateEditRoute() {
  const { code = '' } = useParams()
  return <TemplateMetadataPage code={decodeURIComponent(code)} />
}

function TemplateConfigurationRoute() {
  const { code = '' } = useParams()
  return <TemplateConfigurationPage code={decodeURIComponent(code)} />
}

function TemplateImportRoute() {
  const { code = '' } = useParams()
  const templateCode = decodeURIComponent(code)

  return (
    <ExcelImportPage
      templateCode={templateCode}
      title={`Import — ${templateCode}`}
      description="Chọn file Excel theo template đã cấu hình."
      backPath={getImportExportTemplateListPath()}
    />
  )
}

export default function App() {
  const mainNav = useMemo(
    () =>
      [
        ...DEFAULT_ADMIN_MAIN_NAV.filter((n) => n.id !== 'templates'),
        {
          id: 'ie-templates',
          label: 'Excel templates',
          icon: FileSpreadsheet,
          path: IMPORT_EXPORT_ROUTES.templates,
        },
      ] as AdminNavItem[],
    [],
  )

  return (
    <>
      <NavigateBridge />
      <Routes>
        <Route
          element={
            <AdminLayout
              mainNav={mainNav}
              logoTitle="ImportExport"
              logoSubtitle="Module"
            />
          }
        >
          <Route
            index
            element={<Navigate to={IMPORT_EXPORT_ROUTES.templates} replace />}
          />
          <Route
            path={IMPORT_EXPORT_ROUTES.templates}
            element={<TemplateListPage />}
          />
          <Route
            path={IMPORT_EXPORT_ROUTES.templateCreate}
            element={<TemplateFormPage />}
          />
          <Route
            path={IMPORT_EXPORT_ROUTES.templateEdit}
            element={<TemplateEditRoute />}
          />
          <Route
            path={IMPORT_EXPORT_ROUTES.templateConfiguration}
            element={<TemplateConfigurationRoute />}
          />
          <Route
            path={IMPORT_EXPORT_ROUTES.templateImport}
            element={<TemplateImportRoute />}
          />
          <Route
            path="*"
            element={<Navigate to={IMPORT_EXPORT_ROUTES.templates} replace />}
          />
        </Route>
      </Routes>
    </>
  )
}

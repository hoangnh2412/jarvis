/**
 * Host SPA — AdminLayout + modules từ @platform/core.
 * Setting UI từ @platform/setting (modules/settings/frontend).
 * Files UI từ @platform/files (modules/files/frontend).
 */

import { useEffect, useMemo, useState } from "react";
import {
  Navigate,
  Route,
  Routes,
  useNavigate,
  useParams,
} from "react-router-dom";
import { Building2, FileSpreadsheet, GraduationCap, Layers } from "lucide-react";
import {
  ACCOUNT_ROUTES,
  AdminLayout,
  AccountProfilePage,
  ChangePasswordPage,
  ForgotPasswordPage,
  LoginPage,
  RegisterPage,
  TenantConnectionsPage,
  TenantDetailPage,
  TenantDomainsPage,
  TenantFormPage,
  TenantListPage,
  configureTenantNavigate,
  CraftPdfTemplateListPage,
  CraftPdfEditorPage,
  configureCraftPdfNavigate,
  CRAFT_PDF_ROUTES,
  DASHBOARD_ROUTES,
  DashboardPage,
  RoleListPage,
  ROLE_ROUTES,
  configureRoleNavigate,
  configureFileManagerNavigate,
  ImportPage,
  IMPORT_ROUTES,
  configureImportNavigate,
  PlannerPage,
  PLANNER_ROUTES,
  configurePlannerNavigate,
  TimesheetPage,
  TIMESHEET_ROUTES,
  configureTimesheetNavigate,
  DynamicFormBuilderPage,
  DYNAMIC_FORM_ROUTES,
  DYNAMIC_FORM_DEMO_ID,
  configureDynamicFormNavigate,
  CraftDocBuilderPage,
  CRAFT_DOC_ROUTES,
  configureCraftDocNavigate,
  callLogin,
  notify,
  DEFAULT_ADMIN_MAIN_NAV,
  type AdminNavItem,
} from "@platform/core";
import { clearAccessToken, isAuthenticated, setAccessToken } from "./auth";
import { NotificationBell } from "@platform/notifications";
import { SettingPage } from "@platform/setting";
import {
  OrgListPage,
  ORG_ROUTES,
  configureOrgNavigate,
} from "@platform/orgs";
import { FilesPage, FILE_MANAGER_ROUTES } from "@platform/files";
import { extractLoginResult } from "./auth/apiHelpers";
import { RequireAuth } from "./auth/RequireAuth";
import { isMockAccountCredentials, mockLogin } from "./constants";
import { OnboardingDemoPage } from "./pages/OnboardingDemoPage";

const demoApiKey =
  import.meta.env.VITE_NOTIFICATION_API_KEY ?? "dev-notifications-demo-key";

async function sendDemoNotification() {
  const response = await fetch("/api/signalr-demo/me", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-API-KEY": demoApiKey,
    },
    body: JSON.stringify({
      type: "demo",
      title: "Thông báo demo",
      body: `Gửi lúc ${new Date().toLocaleTimeString("vi-VN")}`,
      data: { actionUrl: "/notifications-demo" },
    }),
  });

  if (!response.ok) {
    const payload = await response.json().catch(() => null);
    throw new Error(
      (payload as { message?: string } | null)?.message ??
      "Không thể gửi thông báo demo.",
    );
  }
}

function NotificationDemoPage() {
  const [sending, setSending] = useState(false);

  const handleSend = async () => {
    setSending(true);
    try {
      await sendDemoNotification();
      notify.success("Đã gửi thông báo demo — xem chuông góc phải.");
    } catch (error) {
      notify.error(
        error instanceof Error ? error.message : "Gửi thông báo demo thất bại.",
      );
    } finally {
      setSending(false);
    }
  };

  return (
    <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <h1 className="m-0 text-xl font-semibold text-slate-900">
        Notification Demo
      </h1>
      <p className="mt-2 text-sm text-slate-600">
        Dùng <code>X-API-KEY</code> (Development) — mở chuông để xem inbox; bấm
        nút bên dưới để push realtime qua SignalR.
      </p>
      <button
        type="button"
        className="mt-4 rounded-lg bg-teal-600 px-4 py-2 text-sm font-semibold text-white hover:bg-teal-700 disabled:opacity-60"
        disabled={sending}
        onClick={() => void handleSend()}
      >
        {sending ? "Đang gửi…" : "Gửi thông báo demo"}
      </button>
    </div>
  );
}

function SampleAdminLayout({ mainNav }: { mainNav: AdminNavItem[] }) {
  const navigate = useNavigate();

  return (
    <AdminLayout
      mainNav={mainNav}
      notificationSlot={<NotificationBell />}
      onLogout={async () => {
        clearAccessToken();
        navigate(ACCOUNT_ROUTES.login, { replace: true });
        return false;
      }}
    />
  );
}

function GuestLoginPage() {
  if (isAuthenticated()) {
    return <Navigate to="/" replace />;
  }

  return (
    <LoginPage
      callback={{
        onSubmit: async (payload) => {
          // Tài khoản mock → không gọi API
          if (isMockAccountCredentials(payload)) {
            const result = await mockLogin(payload);
            setAccessToken(result.token);
            return result;
          }

          // Tài khoản thật → gọi BE
          const response = await callLogin(payload);
          const result = extractLoginResult(response);
          const token = result.tokens?.accessToken;
          if (!token) {
            throw new Error("Server không trả access token.");
          }
          setAccessToken(token);
          return result;
        },
      }}
    />
  );
}
import {
  IMPORT_EXPORT_ROUTES,
  TemplateListPage,
  TemplateFormPage,
  TemplateMetadataPage,
  TemplateConfigurationPage,
  configureImportExportNavigate,
} from '@platform/importexport'
import { StudentListPage } from './pages/StudentListPage'
import { StudentFormPage } from './pages/StudentFormPage'
import { StudentImportPage } from './pages/StudentImportPage'
import { ImportExportSettingPage } from './pages/ImportExportSettingPage'

function TenantNavigateBridge() {
  const navigate = useNavigate();
  useEffect(() => {
    configureTenantNavigate((to) => navigate(to));
  }, [navigate]);
  return null;
}

function CraftPdfNavigateBridge() {
  const navigate = useNavigate();
  useEffect(() => {
    configureCraftPdfNavigate((to) => navigate(to));
  }, [navigate]);
  return null;
}

function RoleNavigateBridge() {
  const navigate = useNavigate();
  useEffect(() => {
    configureRoleNavigate((to) => navigate(to));
  }, [navigate]);
  return null;
}

function OrgNavigateBridge() {
  const navigate = useNavigate();
  useEffect(() => {
    configureOrgNavigate((to) => navigate(to));
  }, [navigate]);
  return null;
}

function FileManagerNavigateBridge() {
  const navigate = useNavigate();
  useEffect(() => {
    configureFileManagerNavigate((to) => navigate(to));
  }, [navigate]);
  return null;
}

function ImportNavigateBridge() {
  const navigate = useNavigate();
  useEffect(() => {
    configureImportNavigate((to) => navigate(to));
  }, [navigate]);
  return null;
}

function PlannerNavigateBridge() {
  const navigate = useNavigate();
  useEffect(() => {
    configurePlannerNavigate((to) => navigate(to));
  }, [navigate]);
  return null;
}

function TimesheetNavigateBridge() {
  const navigate = useNavigate();
  useEffect(() => {
    configureTimesheetNavigate((to) => navigate(to));
  }, [navigate]);
  return null;
}

function DynamicFormNavigateBridge() {
  const navigate = useNavigate();
  useEffect(() => {
    configureDynamicFormNavigate((to) => navigate(to));
  }, [navigate]);
  return null;
}

function CraftDocNavigateBridge() {
  const navigate = useNavigate();
  useEffect(() => {
    configureCraftDocNavigate((to) => navigate(to));
  }, [navigate]);
  return null;
}

function ImportExportNavigateBridge() {
  const navigate = useNavigate();
  useEffect(() => {
    configureImportExportNavigate((to) => navigate(to));
  }, [navigate]);
  return null;
}

function TenantDetailRoute() {
  const { id = "" } = useParams();
  return <TenantDetailPage tenantId={id} />;
}

function TenantEditRoute() {
  const { id = "" } = useParams();
  return <TenantFormPage mode="edit" tenantId={id} />;
}

function TenantConnectionsRoute() {
  const { id = "" } = useParams();
  return <TenantConnectionsPage tenantId={id} />;
}

function TenantDomainsRoute() {
  const { id = "" } = useParams();
  return <TenantDomainsPage tenantId={id} />;
}

function CraftPdfEditorRoute() {
  const { id = "" } = useParams();
  return <CraftPdfEditorPage templateId={id} />;
}
function DynamicFormBuilderRoute() {
  return <DynamicFormBuilderPage formId={DYNAMIC_FORM_DEMO_ID} locale="vi" />;
}


function ImportExportTemplateEditRoute() {
  const { code = '' } = useParams()
  return <TemplateMetadataPage code={decodeURIComponent(code)} />
}

function ImportExportTemplateConfigurationRoute() {
  const { code = '' } = useParams()
  return <TemplateConfigurationPage code={decodeURIComponent(code)} />
}

function PlaceholderPage({ title }: { title: string }) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <h2 className="m-0 text-lg font-semibold text-slate-900">{title}</h2>
      <p className="mt-2 m-0 text-sm text-slate-500">
        Trang placeholder — thay bằng page module của app.
      </p>
    </div>
  );
}

export default function App() {
  const mainNav = useMemo(
    () =>
      [
        ...DEFAULT_ADMIN_MAIN_NAV.filter((n) => n.id !== 'templates'),
        {
          id: 'students',
          label: 'Học sinh',
          icon: GraduationCap,
          path: '/students',
        },
        {
          id: 'orgs',
          label: 'Organization',
          icon: Building2,
          path: ORG_ROUTES.list,
        },
        {
          id: 'ie-templates',
          label: 'Excel templates',
          icon: FileSpreadsheet,
          path: IMPORT_EXPORT_ROUTES.templates,
        },
        {
          id: 'onboarding-demo',
          label: 'Onboarding Demo',
          icon: Layers,
          path: '/onboarding-demo',
        },
      ] as AdminNavItem[],
    [],
  )

  return (
    <>
      <TenantNavigateBridge />
      <CraftPdfNavigateBridge />
      <RoleNavigateBridge />
      <OrgNavigateBridge />
      <FileManagerNavigateBridge />
      <ImportNavigateBridge />
      <PlannerNavigateBridge />
      <TimesheetNavigateBridge />
      <DynamicFormNavigateBridge />
      <CraftDocNavigateBridge />
      <ImportExportNavigateBridge />

      <Routes>
        <Route path={ACCOUNT_ROUTES.login} element={<GuestLoginPage />} />
        <Route path={ACCOUNT_ROUTES.register} element={<RegisterPage />} />
        <Route
          path={ACCOUNT_ROUTES.forgotPassword}
          element={<ForgotPasswordPage />}
        />

        <Route
          path={CRAFT_PDF_ROUTES.list}
          element={
            <RequireAuth>
              <CraftPdfTemplateListPage />
            </RequireAuth>
          }
        />
        <Route
          path={CRAFT_PDF_ROUTES.editor}
          element={
            <RequireAuth>
              <CraftPdfEditorRoute />
            </RequireAuth>
          }
        />

        <Route
          element={
            <RequireAuth>
              <SampleAdminLayout mainNav={mainNav} />
            </RequireAuth>
          }
        >
          <Route index element={<DashboardPage title="Tổng quan" />} />
          <Route path={DASHBOARD_ROUTES.home} element={<DashboardPage />} />
          <Route path="users" element={<PlaceholderPage title="Người dùng" />} />
          <Route path="onboarding-demo" element={<OnboardingDemoPage />} />
          <Route path="notifications-demo" element={<NotificationDemoPage />} />
          <Route
            path="users"
            element={<PlaceholderPage title="Người dùng" />}
          />
          <Route
            path="templates"
            element={<PlaceholderPage title="Biểu mẫu" />}
          />
          <Route
            path="documents"
            element={<PlaceholderPage title="Tài liệu" />}
          />
          <Route path="settings" element={<SettingPage />} />
          <Route
            path="settings/import-export"
            element={<ImportExportSettingPage />}
          />
          <Route path="help" element={<PlaceholderPage title="Trợ giúp" />} />

          <Route path="profile" element={<AccountProfilePage />} />
          <Route path="change-password" element={<ChangePasswordPage />} />

          <Route path="tenants" element={<TenantListPage />} />
          <Route
            path="tenants/create"
            element={<TenantFormPage mode="create" />}
          />
          <Route path="tenants/:id" element={<TenantDetailRoute />} />
          <Route path="tenants/:id/edit" element={<TenantEditRoute />} />
          <Route
            path="tenants/:id/connections"
            element={<TenantConnectionsRoute />}
          />
          <Route path="tenants/:id/domains" element={<TenantDomainsRoute />} />

          <Route
            path={ROLE_ROUTES.list}
            element={<RoleListPage locale="vi" />}
          />

          <Route path={ORG_ROUTES.list} element={<OrgListPage locale="vi" />} />

          <Route
            path={FILE_MANAGER_ROUTES.list}
            element={<FilesPage locale="vi" />}
          />

          <Route
            path={IMPORT_ROUTES.page}
            element={<ImportPage locale="vi" />}
          />

          <Route
            path={PLANNER_ROUTES.page}
            element={<PlannerPage locale="vi" />}
          />

          <Route
            path={TIMESHEET_ROUTES.page}
            element={<TimesheetPage locale="vi" />}
          />

          <Route
            path={DYNAMIC_FORM_ROUTES.page}
            element={<DynamicFormBuilderRoute />}
          />

          <Route
            path={CRAFT_DOC_ROUTES.builder}
            element={<CraftDocBuilderPage locale="vi" />}
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
            element={<ImportExportTemplateEditRoute />}
          />
          <Route
            path={IMPORT_EXPORT_ROUTES.templateConfiguration}
            element={<ImportExportTemplateConfigurationRoute />}
          />

          <Route path="students" element={<StudentListPage />} />
          <Route path="students/import" element={<StudentImportPage />} />
          <Route
            path="students/create"
            element={<StudentFormPage mode="create" />}
          />
          <Route
            path="students/:id/edit"
            element={<StudentFormPage mode="edit" />}
          />

          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </>
  );
}

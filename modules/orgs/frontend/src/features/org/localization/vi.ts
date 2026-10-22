export const viMessages = {
  form: {
    createTitle: 'Tạo organization',
    editTitle: 'Cập nhật organization',
    viewTitle: 'Chi tiết organization',
    code: 'Mã',
    name: 'Tên',
    codePlaceholder: 'acme',
    namePlaceholder: 'Acme Corp',
    saveSuccess: 'Đã lưu organization',
    activateSuccess: 'Đã kích hoạt organization',
    deactivateSuccess: 'Đã ngừng hoạt động organization',
    saveError: 'Không lưu được organization. Vui lòng thử lại.',
    loadError: 'Không tải được organization',
  },
  list: {
    description:
      'Catalog khách hàng cuối trong tenant đang làm việc. Không xóa cứng — chỉ ngừng hoạt động.',
    scopeHint:
      'API cần header X-Org-Id của một organization đang tồn tại. Đặt VITE_ORG_ID hoặc prop orgId.',
    searchPlaceholder: 'Tìm theo mã hoặc tên…',
    statusAll: 'Mọi trạng thái',
    statusActive: 'Đang hoạt động',
    statusInactive: 'Ngừng hoạt động',
    create: 'Thêm organization',
    empty: 'Chưa có organization',
    loading: 'Đang tải…',
  },
  table: {
    code: 'Mã',
    name: 'Tên',
    status: 'Trạng thái',
    actions: 'Thao tác',
    view: 'Xem',
    edit: 'Sửa',
    activate: 'Kích hoạt',
    deactivate: 'Ngừng hoạt động',
  },
  confirm: {
    activateTitle: 'Kích hoạt organization?',
    deactivateTitle: 'Ngừng hoạt động organization?',
    activate: 'Kích hoạt',
    deactivate: 'Ngừng hoạt động',
  },
  detail: {
    code: 'Mã',
    name: 'Tên',
    status: 'Trạng thái',
    tenant: 'Tenant',
    active: 'Đang hoạt động',
    inactive: 'Ngừng hoạt động',
  },
  routes: {
    list: 'Organization',
  },
  menu: {
    orgs: 'Organization',
  },
}

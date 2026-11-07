import type { RJSFSchema, UiSchema } from '@rjsf/utils'

/**
 * Schema demo tận dụng các tính năng RJSF:
 * - types cơ bản, format, enum
 * - nested object
 * - array (addable / removable / orderable / copyable)
 * - dependencies (conditional)
 * - oneOf
 * - uiSchema widgets / order / help / placeholder / enumNames
 */
export const DEMO_RJSF_SCHEMA: RJSFSchema = {
  title: 'Đăng ký tư vấn pháp lý',
  description: 'Form demo RJSF — JSON Schema + uiSchema',
  type: 'object',
  required: ['fullName', 'email', 'practiceArea', 'agree'],
  properties: {
    fullName: {
      type: 'string',
      title: 'Họ và tên',
      minLength: 2,
    },
    email: {
      type: 'string',
      title: 'Email',
      format: 'email',
    },
    phone: {
      type: 'string',
      title: 'Số điện thoại',
    },
    password: {
      type: 'string',
      title: 'Mật khẩu tạm (demo password widget)',
      minLength: 6,
    },
    practiceArea: {
      type: 'string',
      title: 'Lĩnh vực',
      enum: ['corporate', 'litigation', 'ip', 'other'],
    },
    needBy: {
      type: 'string',
      title: 'Ngày cần tư vấn',
      format: 'date',
    },
    priority: {
      type: 'number',
      title: 'Mức ưu tiên (1–5)',
      minimum: 1,
      maximum: 5,
      default: 3,
    },
    notes: {
      type: 'string',
      title: 'Mô tả yêu cầu',
    },
    tags: {
      type: 'array',
      title: 'Tags (multi-select)',
      uniqueItems: true,
      items: {
        type: 'string',
        enum: ['urgent', 'retainer', 'probono', 'vip'],
      },
    },
    contacts: {
      type: 'array',
      title: 'Danh bạ liên hệ (array object)',
      minItems: 1,
      items: {
        type: 'object',
        required: ['name', 'role'],
        properties: {
          name: { type: 'string', title: 'Tên' },
          role: {
            type: 'string',
            title: 'Vai trò',
            enum: ['primary', 'cc', 'billing'],
          },
          email: { type: 'string', title: 'Email', format: 'email' },
        },
      },
    },
    address: {
      type: 'object',
      title: 'Địa chỉ (nested object)',
      properties: {
        city: { type: 'string', title: 'Thành phố' },
        district: { type: 'string', title: 'Quận/Huyện' },
        detail: { type: 'string', title: 'Chi tiết' },
      },
    },
    engagementType: {
      type: 'string',
      title: 'Hình thức',
      enum: ['hourly', 'fixed'],
      default: 'hourly',
    },
    agree: {
      type: 'boolean',
      title: 'Đồng ý điều khoản',
      default: false,
    },
    billing: {
      type: 'object',
      title: 'Thông tin thanh toán (oneOf)',
      oneOf: [
        {
          title: 'Chuyển khoản',
          properties: {
            method: { type: 'string', const: 'bank', default: 'bank' },
            bankName: { type: 'string', title: 'Ngân hàng' },
            accountNo: { type: 'string', title: 'Số tài khoản' },
          },
          required: ['bankName', 'accountNo'],
        },
        {
          title: 'Thẻ',
          properties: {
            method: { type: 'string', const: 'card', default: 'card' },
            cardLast4: {
              type: 'string',
              title: '4 số cuối thẻ',
              pattern: '^[0-9]{4}$',
            },
          },
          required: ['cardLast4'],
        },
      ],
    },
  },
  dependencies: {
    engagementType: {
      oneOf: [
        {
          properties: {
            engagementType: { enum: ['hourly'] },
            hourlyRate: {
              type: 'number',
              title: 'Đơn giá / giờ (conditional)',
              minimum: 0,
            },
          },
        },
        {
          properties: {
            engagementType: { enum: ['fixed'] },
            fixedFee: {
              type: 'number',
              title: 'Phí trọn gói (conditional)',
              minimum: 0,
            },
          },
        },
      ],
    },
  },
}

export const DEMO_RJSF_UI_SCHEMA: UiSchema = {
  'ui:order': [
    'fullName',
    'email',
    'phone',
    'password',
    'practiceArea',
    'needBy',
    'priority',
    'tags',
    'notes',
    'contacts',
    'address',
    'engagementType',
    'hourlyRate',
    'fixedFee',
    'billing',
    'agree',
    '*',
  ],
  fullName: {
    'ui:autofocus': true,
    'ui:placeholder': 'Nhập họ tên',
  },
  email: {
    'ui:placeholder': 'email@company.com',
  },
  password: {
    'ui:widget': 'password',
    'ui:help': 'Chỉ demo widget password của RJSF',
  },
  practiceArea: {
    'ui:placeholder': 'Chọn lĩnh vực',
    'ui:enumNames': ['Corporate', 'Litigation', 'Sở hữu trí tuệ', 'Khác'],
  },
  priority: {
    'ui:widget': 'range',
  },
  notes: {
    'ui:widget': 'textarea',
    'ui:options': { rows: 4 },
    'ui:help': 'Tóm tắt vụ việc cần hỗ trợ',
  },
  tags: {
    'ui:widget': 'checkboxes',
    'ui:enumNames': ['Khẩn', 'Retainer', 'Pro bono', 'VIP'],
  },
  contacts: {
    'ui:options': {
      orderable: true,
      addable: true,
      removable: true,
      copyable: true,
    },
    items: {
      email: { 'ui:placeholder': 'contact@example.com' },
      role: {
        'ui:enumNames': ['Chính', 'CC', 'Billing'],
      },
    },
  },
  address: {
    detail: {
      'ui:widget': 'textarea',
      'ui:options': { rows: 2 },
    },
  },
  engagementType: {
    'ui:enumNames': ['Theo giờ', 'Trọn gói'],
  },
  agree: {
    'ui:help': 'Bắt buộc để gửi form',
  },
}

export const DEMO_RJSF_FORM_DATA: Record<string, unknown> = {
  priority: 3,
  engagementType: 'hourly',
  tags: [],
  contacts: [{ name: '', role: 'primary', email: '' }],
  agree: false,
}

/** Preset snippets để chèn nhanh trong builder */
export type RjsfFeaturePreset = {
  id: string
  label: string
  description: string
  mergeSchema: RJSFSchema
  mergeUiSchema?: UiSchema
}

export const RJSF_FEATURE_PRESETS: RjsfFeaturePreset[] = [
  {
    id: 'array-string',
    label: 'Array (string)',
    description: 'Danh sách chuỗi + add/remove',
    mergeSchema: {
      properties: {
        keywords: {
          type: 'array',
          title: 'Từ khóa',
          items: { type: 'string' },
        },
      },
    },
    mergeUiSchema: {
      keywords: {
        'ui:options': { orderable: true, addable: true, removable: true },
      },
    },
  },
  {
    id: 'array-object',
    label: 'Array (object)',
    description: 'Bảng dòng object',
    mergeSchema: {
      properties: {
        lineItems: {
          type: 'array',
          title: 'Chi tiết',
          items: {
            type: 'object',
            properties: {
              label: { type: 'string', title: 'Nhãn' },
              amount: { type: 'number', title: 'Số tiền' },
            },
          },
        },
      },
    },
  },
  {
    id: 'nested-object',
    label: 'Nested object',
    description: 'Object lồng nhau',
    mergeSchema: {
      properties: {
        meta: {
          type: 'object',
          title: 'Meta',
          properties: {
            source: { type: 'string', title: 'Nguồn' },
            campaign: { type: 'string', title: 'Campaign' },
          },
        },
      },
    },
  },
  {
    id: 'dependencies',
    label: 'Dependencies',
    description: 'Field hiện theo điều kiện',
    mergeSchema: {
      properties: {
        hasBudget: {
          type: 'boolean',
          title: 'Có ngân sách?',
          default: false,
        },
      },
      dependencies: {
        hasBudget: {
          oneOf: [
            {
              properties: {
                hasBudget: { enum: [true] },
                budget: { type: 'number', title: 'Ngân sách' },
              },
            },
            {
              properties: {
                hasBudget: { enum: [false] },
              },
            },
          ],
        },
      },
    },
  },
  {
    id: 'oneof',
    label: 'oneOf',
    description: 'Chọn một nhánh schema',
    mergeSchema: {
      properties: {
        payment: {
          type: 'object',
          title: 'Thanh toán',
          oneOf: [
            {
              title: 'Cash',
              properties: {
                kind: { const: 'cash', default: 'cash' },
                note: { type: 'string', title: 'Ghi chú' },
              },
            },
            {
              title: 'Invoice',
              properties: {
                kind: { const: 'invoice', default: 'invoice' },
                invoiceNo: { type: 'string', title: 'Số hóa đơn' },
              },
              required: ['invoiceNo'],
            },
          ],
        },
      },
    },
  },
  {
    id: 'checkboxes',
    label: 'Checkboxes',
    description: 'Multi enum + ui:widget checkboxes',
    mergeSchema: {
      properties: {
        channels: {
          type: 'array',
          title: 'Kênh',
          uniqueItems: true,
          items: {
            type: 'string',
            enum: ['email', 'phone', 'chat'],
          },
        },
      },
    },
    mergeUiSchema: {
      channels: {
        'ui:widget': 'checkboxes',
        'ui:enumNames': ['Email', 'Điện thoại', 'Chat'],
      },
    },
  },
]

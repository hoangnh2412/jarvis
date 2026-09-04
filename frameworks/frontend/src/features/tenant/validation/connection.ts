import { z } from 'zod'
import { DbProviderType } from '../types'

export const tenantConnectionFormSchema = z
  .object({
    providerType: z.union([
      z.literal(DbProviderType.Postgres),
      z.literal(DbProviderType.SqlServer),
      z.literal(DbProviderType.MySql),
    ]),
    connectionString: z
      .string()
      .trim()
      .min(1, 'Vui lòng nhập connection string'),
    partitionFrom: z.string().nullable().optional(),
    partitionTo: z.string().nullable().optional(),
    isDefault: z.boolean(),
  })
  .refine(
    (data) => {
      if (!data.partitionFrom || !data.partitionTo) return true
      return data.partitionFrom <= data.partitionTo
    },
    {
      message: 'PartitionFrom phải nhỏ hơn hoặc bằng PartitionTo',
      path: ['partitionTo'],
    },
  )

export type TenantConnectionFormData = z.infer<
  typeof tenantConnectionFormSchema
>

export const tenantConnectionFormDefaultValues: TenantConnectionFormData = {
  providerType: DbProviderType.Postgres,
  connectionString: '',
  partitionFrom: null,
  partitionTo: null,
  isDefault: false,
}

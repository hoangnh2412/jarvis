import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import {
  Button,
  InputText,
  getErrorMessage,
  notify,
} from '@platform/core'
import { ImportExportPageShell } from '@platform/importexport'
import {
  btnOutlinedClass,
  btnPrimaryClass,
  fieldInputClass,
} from '../components/fieldStyles'
import {
  createStudent,
  getStudent,
  updateStudent,
} from '../services/students'

export type StudentFormPageProps = {
  mode: 'create' | 'edit'
}

export function StudentFormPage({ mode }: StudentFormPageProps) {
  const navigate = useNavigate()
  const { id = '' } = useParams()
  const [name, setName] = useState('')
  const [age, setAge] = useState('18')
  const [loading, setLoading] = useState(mode === 'edit')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (mode !== 'edit' || !id) return
    setLoading(true)
    void getStudent(id)
      .then((student) => {
        setName(student.name)
        setAge(String(student.age))
      })
      .catch((err) =>
        notify.error(getErrorMessage(err, 'Không tải được học sinh')),
      )
      .finally(() => setLoading(false))
  }, [mode, id])

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    const trimmedName = name.trim()
    if (!trimmedName) {
      notify.error('Tên là bắt buộc')
      return
    }
    const ageNum = Number(age)
    if (Number.isNaN(ageNum) || ageNum < 0) {
      notify.error('Tuổi không hợp lệ')
      return
    }

    setSaving(true)
    try {
      if (mode === 'create') {
        await createStudent({ name: trimmedName, age: ageNum })
        notify.success('Đã tạo học sinh')
      } else {
        await updateStudent(id, { name: trimmedName, age: ageNum })
        notify.success('Đã cập nhật học sinh')
      }
      navigate('/students')
    } catch (err) {
      notify.error(
        getErrorMessage(
          err,
          mode === 'create' ? 'Tạo học sinh thất bại' : 'Cập nhật thất bại',
        ),
      )
    } finally {
      setSaving(false)
    }
  }

  const title = mode === 'create' ? 'Thêm học sinh' : 'Sửa học sinh'

  return (
    <ImportExportPageShell
      title={title}
      description="Dữ liệu được lưu vào bảng Student (tenant từ X-Tenant-Id)."
      actions={
        <Button
          type="button"
          unstyled
          className={btnOutlinedClass}
          onClick={() => navigate('/students')}
        >
          Quay lại
        </Button>
      }
    >
      {loading ? (
        <p className="text-sm text-slate-500">Đang tải…</p>
      ) : (
        <form
          onSubmit={(e) => void onSubmit(e)}
          className="max-w-md space-y-4 rounded-xl border border-slate-200 bg-white p-4 shadow-sm"
        >
          <label className="block text-sm">
            <span className="mb-1 block font-medium text-slate-700">Tên</span>
            <InputText
              unstyled
              value={name}
              onChange={(e: ChangeEvent<HTMLInputElement>) =>
                setName(e.target.value)
              }
              className={fieldInputClass}
              placeholder="Nguyễn Văn A"
              required
            />
          </label>

          <label className="block text-sm">
            <span className="mb-1 block font-medium text-slate-700">Tuổi</span>
            <InputText
              unstyled
              type="number"
              min={0}
              max={150}
              value={age}
              onChange={(e: ChangeEvent<HTMLInputElement>) =>
                setAge(e.target.value)
              }
              className={fieldInputClass}
              required
            />
          </label>

          <Button
            type="submit"
            unstyled
            disabled={saving}
            className={btnPrimaryClass}
          >
            {saving ? 'Đang lưu…' : mode === 'create' ? 'Tạo' : 'Cập nhật'}
          </Button>
        </form>
      )}
    </ImportExportPageShell>
  )
}

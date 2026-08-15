import {
  ToastContainer,
  toast,
  type ToastContainerProps,
  type ToastOptions as ToastifyOptions,
} from 'react-toastify'

export type ToasterProps = Partial<ToastContainerProps>

export type ToastOptions = ToastifyOptions

const defaultOptions: ToastifyOptions = {
  position: 'top-right',
  autoClose: 3000,
  hideProgressBar: true,
  closeOnClick: true,
  pauseOnHover: true,
  draggable: false,
  className: 'kit-toast',
}

/** Mount 1 lần ở root app. */
export function Toaster({
  position = 'top-right',
  autoClose = 3000,
  newestOnTop = true,
  closeOnClick = true,
  pauseOnHover = true,
  draggable = false,
  hideProgressBar = true,
  theme = 'light',
  className = 'kit-toast-container',
  toastClassName = 'kit-toast',
  ...rest
}: ToasterProps) {
  return (
    <ToastContainer
      position={position}
      autoClose={autoClose}
      newestOnTop={newestOnTop}
      closeOnClick={closeOnClick}
      pauseOnHover={pauseOnHover}
      draggable={draggable}
      hideProgressBar={hideProgressBar}
      theme={theme}
      className={className}
      toastClassName={toastClassName}
      {...rest}
    />
  )
}

export const AppToaster = Toaster

export const notify = {
  success(message: string, options?: ToastOptions) {
    return toast.success(message, { ...defaultOptions, ...options })
  },
  error(message: string, options?: ToastOptions) {
    return toast.error(message, { ...defaultOptions, ...options })
  },
  info(message: string, options?: ToastOptions) {
    return toast.info(message, { ...defaultOptions, ...options })
  },
  warning(message: string, options?: ToastOptions) {
    return toast.warning(message, { ...defaultOptions, ...options })
  },
}

export { toast }

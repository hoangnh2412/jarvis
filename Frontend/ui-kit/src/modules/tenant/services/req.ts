import axios from 'axios'
import { BASE_URL_TENANT } from '../constants'

const instance = axios.create({
  baseURL: BASE_URL_TENANT,
  withCredentials: true,
})

instance.interceptors.request.use(
  (config) => {
    return config
  },
  (error) => {
    return Promise.reject(error)
  },
)

instance.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    const normalizeError = error.response?.data ?? error
    return Promise.reject(normalizeError)
  },
)

export default instance

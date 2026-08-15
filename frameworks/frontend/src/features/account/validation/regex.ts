export const REGEX = {
  PHONE_VN: /^(03|05|07|08|09)\d{8}$/,
  PHONE_10_DIGITS: /^\d{10}$/,
  IDENTITY_NUMBER: /^(\d{9}|\d{12})$/,
  IDENTITY_CCCD: /^\d{12}$/,
  ONLY_NUMBER: /^\d+$/,
  ALL_ZERO: /^0+$/,
  EMAIL: /^[a-zA-Z0-9._%+-]+@[a-zA-Z]+([.-][a-zA-Z]+)*\.[a-zA-Z]+$/,
  PERSON_NAME: /^[\p{L}\s]+$/u,
}

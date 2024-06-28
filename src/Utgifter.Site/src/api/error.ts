export type Error = {
  status: number
  message: string
  errors?: {
    [key: string]: string[]
  }
}

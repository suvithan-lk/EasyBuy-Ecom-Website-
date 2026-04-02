import axios from 'axios'

const client = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5233/api',
  timeout: 10000,
})

const unwrap = async (request) => {
  try {
    const { data } = await request
    return data
  } catch (error) {
    throw new Error(error.response?.data?.message ?? error.message ?? 'Request failed.')
  }
}

export const easyBuyApi = {
  getHome() {
    return unwrap(client.get('/catalog/home'))
  },
  getProducts(params) {
    return unwrap(client.get('/catalog/products', { params }))
  },
  getProduct(productId) {
    return unwrap(client.get(`/catalog/products/${productId}`))
  },
  getCart() {
    return unwrap(client.get('/cart'))
  },
  updateCartItem(payload) {
    return unwrap(client.post('/cart/items', payload))
  },
  removeCartItem(productId) {
    return unwrap(client.delete(`/cart/items/${productId}`))
  },
  getWishlist() {
    return unwrap(client.get('/wishlist'))
  },
  toggleWishlist(productId) {
    return unwrap(client.post(`/wishlist/${productId}`))
  },
  validateCoupon(payload) {
    return unwrap(client.post('/coupons/validate', payload))
  },
  checkout(payload) {
    return unwrap(client.post('/orders/checkout', payload))
  },
  getOrders() {
    return unwrap(client.get('/orders'))
  },
  getOrder(orderId) {
    return unwrap(client.get(`/orders/${orderId}`))
  },
  getAdminSummary() {
    return unwrap(client.get('/admin/summary'))
  },
}

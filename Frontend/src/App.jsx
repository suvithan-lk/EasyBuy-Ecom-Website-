import { startTransition, useDeferredValue, useEffect, useMemo, useState } from 'react'
import { HashRouter, Navigate, Route, Routes } from 'react-router-dom'
import { easyBuyApi } from './api/easyBuyApi'
import MarketplaceHeader from './components/MarketplaceHeader'
import StorefrontPage from './components/StorefrontPage'
import CheckoutPage from './components/CheckoutPage'
import AuthPage from './components/AuthPage'
import ProductModal from './components/ProductModal'
import OrderTrackingPage from './components/OrderTrackingPage'
import { heroMedia } from './data/productMedia'
import { sriLankaDistricts } from './data/sriLankaDistricts'

const sessionKey = 'easybuy-session'
const deliveryLocationKey = 'easybuy-delivery-location'

const currencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
})

const emptyCart = {
  items: [],
  itemCount: 0,
  subtotal: 0,
  discount: 0,
  shipping: 0,
  total: 0,
  qualifiesForFreeShipping: false,
  freeShippingGap: 0,
}

const emptyOrders = { items: [] }
const emptyWishlist = { items: [], count: 0 }
const emptyCatalog = { items: [], totalItems: 0, search: '', category: 'all', sort: 'featured' }
const emptyAdmin = {
  grossRevenue: 0,
  orderCount: 0,
  unitsSold: 0,
  averageOrderValue: 0,
  inventoryAlerts: [],
  categoryPerformance: [],
  recentOrders: [],
}

const baseCheckoutForm = {
  name: '',
  email: '',
  addressLine: '14 Lake View Avenue',
  city: 'Colombo',
  postalCode: '00500',
  paymentMethod: 'Card',
  notes: '',
}

const formatCurrency = (value) => currencyFormatter.format(value ?? 0)

const formatDate = (value) =>
  new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  }).format(new Date(value))

const buildCheckoutPreview = (cart, coupon, home) => {
  const discount = coupon?.discount ?? 0
  const adjustedSubtotal = Math.max(cart.subtotal - discount, 0)
  const threshold = home?.freeShippingThreshold ?? Number.POSITIVE_INFINITY
  const standardRate = home?.standardShippingRate ?? cart.shipping ?? 0
  const shipping = cart.itemCount === 0 ? 0 : adjustedSubtotal >= threshold ? 0 : standardRate

  return {
    subtotal: cart.subtotal,
    discount,
    shipping,
    total: adjustedSubtotal + shipping,
  }
}

const getSession = () => {
  try {
    const raw = localStorage.getItem(sessionKey)
    return raw ? JSON.parse(raw) : null
  } catch {
    return null
  }
}

const getDeliveryLocation = () => {
  try {
    const stored = localStorage.getItem(deliveryLocationKey)
    return stored && sriLankaDistricts.includes(stored) ? stored : 'Colombo'
  } catch {
    return 'Colombo'
  }
}

const enrichProduct = (product) => ({
  ...product,
  imageUrl: product.imageUrl || heroMedia.banner,
  gallery:
    product.gallery && product.gallery.length > 0
      ? product.gallery.filter(Boolean)
      : [product.imageUrl || heroMedia.banner],
})

const enrichHome = (home) => ({
  ...home,
  categories: home.categories.map((category) => ({
    ...category,
    imageUrl: category.imageUrl || heroMedia.banner,
  })),
  featuredProducts: home.featuredProducts.map(enrichProduct),
  spotlightProducts: home.spotlightProducts.map(enrichProduct),
})

const enrichCatalog = (catalog) => ({
  ...catalog,
  items: catalog.items.map(enrichProduct),
})

const enrichWishlist = (wishlist) => ({
  ...wishlist,
  items: wishlist.items.map(enrichProduct),
})

const enrichCart = (cart) => ({
  ...cart,
  items: cart.items.map((item) => ({
    ...item,
    imageUrl: item.imageUrl || heroMedia.banner,
  })),
})

function App() {
  const [session, setSession] = useState(getSession)
  const [deliveryLocation, setDeliveryLocation] = useState(getDeliveryLocation)
  const [home, setHome] = useState(null)
  const [catalog, setCatalog] = useState(emptyCatalog)
  const [cart, setCart] = useState(emptyCart)
  const [wishlist, setWishlist] = useState(emptyWishlist)
  const [orders, setOrders] = useState(emptyOrders)
  const [admin, setAdmin] = useState(emptyAdmin)
  const [search, setSearch] = useState('')
  const [selectedCategory, setSelectedCategory] = useState('all')
  const [selectedSort, setSelectedSort] = useState('featured')
  const [status, setStatus] = useState(null)
  const [busyProductId, setBusyProductId] = useState(null)
  const [selectedProduct, setSelectedProduct] = useState(null)
  const [detailLoading, setDetailLoading] = useState(false)
  const [catalogLoading, setCatalogLoading] = useState(true)
  const [booting, setBooting] = useState(true)
  const [couponCode, setCouponCode] = useState('')
  const [couponPreview, setCouponPreview] = useState(null)
  const [applyingCoupon, setApplyingCoupon] = useState(false)
  const [placingOrder, setPlacingOrder] = useState(false)
  const [checkoutForm, setCheckoutForm] = useState(baseCheckoutForm)
  const deferredSearch = useDeferredValue(search)

  const checkoutPreview = useMemo(
    () => buildCheckoutPreview(cart, couponPreview, home),
    [cart, couponPreview, home]
  )

  useEffect(() => {
    let cancelled = false

    const loadShell = async () => {
      try {
        const [homeData, cartData, wishlistData, ordersData, adminData] = await Promise.all([
          easyBuyApi.getHome(),
          easyBuyApi.getCart(),
          easyBuyApi.getWishlist(),
          easyBuyApi.getOrders(),
          easyBuyApi.getAdminSummary(),
        ])

        if (cancelled) {
          return
        }

        startTransition(() => {
          setHome(enrichHome(homeData))
          setCart(enrichCart(cartData))
          setWishlist(enrichWishlist(wishlistData))
          setOrders(ordersData)
          setAdmin(adminData)
        })
      } catch (error) {
        if (!cancelled) {
          setStatus({ tone: 'error', message: error.message })
        }
      } finally {
        if (!cancelled) {
          setBooting(false)
        }
      }
    }

    loadShell()

    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    let cancelled = false

    const loadCatalog = async () => {
      setCatalogLoading(true)

      try {
        const data = await easyBuyApi.getProducts({
          search: deferredSearch || undefined,
          category: selectedCategory === 'all' ? undefined : selectedCategory,
          sort: selectedSort,
        })

        if (cancelled) {
          return
        }

        startTransition(() => {
          setCatalog(enrichCatalog(data))
        })
      } catch (error) {
        if (!cancelled) {
          setStatus({ tone: 'error', message: error.message })
        }
      } finally {
        if (!cancelled) {
          setCatalogLoading(false)
        }
      }
    }

    loadCatalog()

    return () => {
      cancelled = true
    }
  }, [deferredSearch, selectedCategory, selectedSort])

  useEffect(() => {
    setCheckoutForm((current) => ({
      ...current,
      name: session?.name || home?.shopperName || current.name,
      email: session?.email || current.email,
    }))
  }, [session, home])

  useEffect(() => {
    localStorage.setItem(deliveryLocationKey, deliveryLocation)
    setCheckoutForm((current) => ({
      ...current,
      city: deliveryLocation,
    }))
  }, [deliveryLocation])

  const refreshShell = async () => {
    const [homeData, cartData, wishlistData, ordersData, adminData] = await Promise.all([
      easyBuyApi.getHome(),
      easyBuyApi.getCart(),
      easyBuyApi.getWishlist(),
      easyBuyApi.getOrders(),
      easyBuyApi.getAdminSummary(),
    ])

    startTransition(() => {
      setHome(enrichHome(homeData))
      setCart(enrichCart(cartData))
      setWishlist(enrichWishlist(wishlistData))
      setOrders(ordersData)
      setAdmin(adminData)
    })
  }

  const refreshCatalog = async () => {
    const data = await easyBuyApi.getProducts({
      search: deferredSearch || undefined,
      category: selectedCategory === 'all' ? undefined : selectedCategory,
      sort: selectedSort,
    })

    startTransition(() => {
      setCatalog(enrichCatalog(data))
    })
  }

  const persistSession = (nextSession) => {
    localStorage.setItem(sessionKey, JSON.stringify(nextSession))
    setSession(nextSession)
  }

  const clearSession = () => {
    localStorage.removeItem(sessionKey)
    setSession(null)
    setStatus({ tone: 'info', message: 'Signed out of the demo account.' })
  }

  const authenticate = (payload) => {
    const nextSession = {
      name: payload.name,
      email: payload.email,
    }

    persistSession(nextSession)
    setStatus({ tone: 'success', message: `Welcome, ${payload.name}.` })
    return nextSession
  }

  const updateWishlistFlags = (productId, isWishlisted) => {
    setCatalog((current) => ({
      ...current,
      items: current.items.map((item) =>
        item.id === productId ? { ...item, isWishlisted } : item
      ),
    }))

    setHome((current) =>
      current
        ? {
            ...current,
            featuredProducts: current.featuredProducts.map((item) =>
              item.id === productId ? { ...item, isWishlisted } : item
            ),
            spotlightProducts: current.spotlightProducts.map((item) =>
              item.id === productId ? { ...item, isWishlisted } : item
            ),
          }
        : current
    )

    setSelectedProduct((current) =>
      current && current.id === productId ? { ...current, isWishlisted } : current
    )
  }

  const viewProduct = async (productId) => {
    setDetailLoading(true)
    setSelectedProduct(null)

    try {
      const product = await easyBuyApi.getProduct(productId)
      setSelectedProduct(enrichProduct(product))
    } catch (error) {
      setStatus({ tone: 'error', message: error.message })
    } finally {
      setDetailLoading(false)
    }
  }

  const addToCart = async (productId, quantityStep = 1) => {
    const currentLine = cart.items.find((item) => item.productId === productId)
    const nextQuantity = (currentLine?.quantity ?? 0) + quantityStep

    if (nextQuantity <= 0) {
      return
    }

    setBusyProductId(productId)

    try {
      const nextCart = await easyBuyApi.updateCartItem({ productId, quantity: nextQuantity })
      setCart(enrichCart(nextCart))
      setStatus({ tone: 'success', message: 'Cart updated.' })
    } catch (error) {
      setStatus({ tone: 'error', message: error.message })
    } finally {
      setBusyProductId(null)
    }
  }

  const updateCartQuantity = async (productId, nextQuantity) => {
    setBusyProductId(productId)

    try {
      const nextCart =
        nextQuantity <= 0
          ? await easyBuyApi.removeCartItem(productId)
          : await easyBuyApi.updateCartItem({ productId, quantity: nextQuantity })

      setCart(enrichCart(nextCart))
      setCouponPreview(null)
      setCouponCode('')
    } catch (error) {
      setStatus({ tone: 'error', message: error.message })
      throw error
    } finally {
      setBusyProductId(null)
    }
  }

  const toggleWishlist = async (productId) => {
    setBusyProductId(productId)

    try {
      const nextWishlist = await easyBuyApi.toggleWishlist(productId)
      const enrichedWishlist = enrichWishlist(nextWishlist)
      const isWishlisted = enrichedWishlist.items.some((item) => item.id === productId)

      setWishlist(enrichedWishlist)
      updateWishlistFlags(productId, isWishlisted)
      setStatus({
        tone: 'success',
        message: isWishlisted ? 'Saved to wishlist.' : 'Removed from wishlist.',
      })
    } catch (error) {
      setStatus({ tone: 'error', message: error.message })
    } finally {
      setBusyProductId(null)
    }
  }

  const applyCoupon = async () => {
    if (!couponCode.trim()) {
      setStatus({ tone: 'error', message: 'Enter a coupon code first.' })
      throw new Error('Missing coupon code.')
    }

    setApplyingCoupon(true)

    try {
      const preview = await easyBuyApi.validateCoupon({
        code: couponCode,
        subtotal: cart.subtotal,
      })

      setCouponPreview(preview)
      setStatus({ tone: 'success', message: `${preview.code} applied.` })
      return preview
    } catch (error) {
      setCouponPreview(null)
      setStatus({ tone: 'error', message: error.message })
      throw error
    } finally {
      setApplyingCoupon(false)
    }
  }

  const placeOrder = async () => {
    if (cart.itemCount === 0) {
      const error = new Error('Add products before checking out.')
      setStatus({ tone: 'error', message: error.message })
      throw error
    }

    setPlacingOrder(true)

    try {
      const result = await easyBuyApi.checkout({
        ...checkoutForm,
        couponCode: couponPreview?.code ?? null,
      })

      setCouponCode('')
      setCouponPreview(null)
      setSelectedProduct(null)
      setCheckoutForm((current) => ({ ...current, notes: '' }))
      await refreshShell()
      await refreshCatalog()
      setStatus({
        tone: 'success',
        message: `Order ${result.order.orderNumber} placed successfully.`,
      })
      return result
    } catch (error) {
      setStatus({ tone: 'error', message: error.message })
      throw error
    } finally {
      setPlacingOrder(false)
    }
  }

  if (booting) {
    return (
      <div className="boot-shell">
        <div className="boot-card">
          <span className="section-kicker">EasyBuy Marketplace</span>
          <h1>Preparing storefront, checkout, and account pages.</h1>
          <p>The UI is loading catalog, basket, wishlist, orders, and admin metrics from the API.</p>
        </div>
      </div>
    )
  }

  return (
    <HashRouter>
      <div className="site-shell">
        <MarketplaceHeader
          session={session}
          cartCount={cart.itemCount}
          search={search}
          setSearch={setSearch}
          deliveryLocation={deliveryLocation}
          setDeliveryLocation={setDeliveryLocation}
          onLogout={clearSession}
        />

        {status ? (
          <div className={`global-status ${status.tone}`}>
            <span>{status.message}</span>
            <button type="button" onClick={() => setStatus(null)}>
              Dismiss
            </button>
          </div>
        ) : null}

        <Routes>
          <Route
            path="/"
            element={
              <StorefrontPage
                heroMedia={heroMedia}
                session={session}
                home={home}
                catalog={catalog}
                cart={cart}
                wishlist={wishlist}
                orders={orders}
                admin={admin}
                search={search}
                setSearch={setSearch}
                selectedCategory={selectedCategory}
                setSelectedCategory={setSelectedCategory}
                selectedSort={selectedSort}
                setSelectedSort={setSelectedSort}
                catalogLoading={catalogLoading}
                busyProductId={busyProductId}
                onViewProduct={viewProduct}
                onAddToCart={addToCart}
                onToggleWishlist={toggleWishlist}
                formatCurrency={formatCurrency}
                formatDate={formatDate}
              />
            }
          />
          <Route
            path="/checkout"
            element={
              <CheckoutPage
                session={session}
                heroMedia={heroMedia}
                home={home}
                cart={cart}
                couponCode={couponCode}
                setCouponCode={setCouponCode}
                couponPreview={couponPreview}
                applyingCoupon={applyingCoupon}
                checkoutPreview={checkoutPreview}
                checkoutForm={checkoutForm}
                setCheckoutForm={setCheckoutForm}
                deliveryLocation={deliveryLocation}
                setDeliveryLocation={setDeliveryLocation}
                districts={sriLankaDistricts}
                placingOrder={placingOrder}
                onCartQuantity={updateCartQuantity}
                onApplyCoupon={applyCoupon}
                onPlaceOrder={placeOrder}
                formatCurrency={formatCurrency}
              />
            }
          />
          <Route
            path="/tracking"
            element={
              <OrderTrackingPage
                orders={orders}
                session={session}
                formatCurrency={formatCurrency}
                formatDate={formatDate}
              />
            }
          />
          <Route
            path="/tracking/:orderId"
            element={
              <OrderTrackingPage
                orders={orders}
                session={session}
                formatCurrency={formatCurrency}
                formatDate={formatDate}
              />
            }
          />
          <Route
            path="/login"
            element={<AuthPage mode="login" heroMedia={heroMedia} onSubmitAuth={authenticate} />}
          />
          <Route
            path="/register"
            element={<AuthPage mode="register" heroMedia={heroMedia} onSubmitAuth={authenticate} />}
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>

        <ProductModal
          product={selectedProduct}
          loading={detailLoading}
          onClose={() => setSelectedProduct(null)}
          onToggleWishlist={toggleWishlist}
          onAddToCart={addToCart}
          formatCurrency={formatCurrency}
          formatDate={formatDate}
        />
      </div>
    </HashRouter>
  )
}

export default App

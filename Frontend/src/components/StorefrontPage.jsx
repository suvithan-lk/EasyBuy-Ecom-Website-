import { Link } from 'react-router-dom'
import ProductCard from './ProductCard'

const StorefrontPage = ({
  heroMedia,
  session,
  home,
  catalog,
  cart,
  wishlist,
  orders,
  admin,
  selectedCategory,
  setSelectedCategory,
  selectedSort,
  setSelectedSort,
  catalogLoading,
  busyProductId,
  onViewProduct,
  onAddToCart,
  onToggleWishlist,
  formatCurrency,
  formatDate,
}) => {
  const featuredProduct = home?.featuredProducts?.[0]

  return (
  <main className="marketplace-page">
    <section className="hero-banner-grid">
      <article className="hero-banner">
        <img src={heroMedia.banner} alt="EasyBuy hero collection" />
        <div className="hero-banner-copy">
          <span className="section-kicker">Mega Electronics Sale</span>
          <h1>Amazon-style shopping flow with Daraz-inspired color and density.</h1>
          <p>
            Explore catalog browsing, image-led listings, dedicated checkout, and account pages
            in one storefront.
          </p>
          <div className="hero-banner-actions">
            <Link to="/checkout" className="primary-action">
              Go to Checkout
            </Link>
            <button
              type="button"
              className="secondary-action"
              disabled={!featuredProduct}
              onClick={() => featuredProduct && onViewProduct(featuredProduct.id)}
            >
              Quick View Featured Item
            </button>
          </div>
        </div>
      </article>

      <article className="hero-side-card">
        <img src={heroMedia.deals} alt="Online shopping setup" />
        <div>
          <span className="section-kicker">Basket snapshot</span>
          <h3>{cart.itemCount} items in cart</h3>
          <p>Total now {formatCurrency(cart.total)} before final payment.</p>
          <Link to="/checkout">Continue to payment page</Link>
        </div>
      </article>

      <article className="hero-side-card accent-card">
        <div>
          <span className="section-kicker">Account</span>
          <h3>{session ? `Welcome back, ${session.name.split(' ')[0]}` : 'Login or Register'}</h3>
          <p>
            The storefront now includes dedicated authentication screens, not just a single demo
            page.
          </p>
          <div className="mini-action-row">
            <Link to="/login">Login</Link>
            <Link to="/register">Register</Link>
          </div>
        </div>
      </article>
    </section>

    <section className="category-strip" id="categories">
      {home?.categories.map((category) => (
        <button
          key={category.slug}
          type="button"
          className={`category-card ${selectedCategory === category.slug ? 'active' : ''}`}
          onClick={() => setSelectedCategory(category.slug)}
        >
          <img src={category.imageUrl} alt={category.name} loading="lazy" />
          <div>
            <strong>{category.name}</strong>
            <small>{category.productCount} products</small>
          </div>
        </button>
      ))}
      <button
        type="button"
        className={`category-card ${selectedCategory === 'all' ? 'active' : ''}`}
        onClick={() => setSelectedCategory('all')}
      >
        <img src={heroMedia.banner} alt="All categories" loading="lazy" />
        <div>
          <strong>All departments</strong>
          <small>Browse complete inventory</small>
        </div>
      </button>
    </section>

    <section className="product-section" id="products">
      <div className="section-header-row">
        <div>
          <span className="section-kicker">Best sellers and curated picks</span>
          <h2>{catalogLoading ? 'Refreshing products...' : `${catalog.totalItems} marketplace items`}</h2>
        </div>

        <div className="filter-row">
          <button type="button" onClick={() => setSelectedCategory('all')}>
            Reset category
          </button>
          <select value={selectedSort} onChange={(event) => setSelectedSort(event.target.value)}>
            <option value="featured">Featured</option>
            <option value="rating">Top rated</option>
            <option value="newest">New arrivals</option>
            <option value="price-asc">Price: low to high</option>
            <option value="price-desc">Price: high to low</option>
          </select>
        </div>
      </div>

      <div className="product-layout">
        <div className="product-grid-market">
          {catalog.items.map((product) => (
            <ProductCard
              key={product.id}
              product={product}
              onView={onViewProduct}
              onAddToCart={onAddToCart}
              onToggleWishlist={onToggleWishlist}
              formatCurrency={formatCurrency}
              busyProductId={busyProductId}
            />
          ))}
        </div>

        <aside className="store-side-column">
          <article className="info-panel">
            <span className="section-kicker">Saved items</span>
            <h3>{wishlist.count} products in wishlist</h3>
            <div className="compact-list">
              {wishlist.items.slice(0, 3).map((item) => (
                <button key={item.id} type="button" className="compact-item" onClick={() => onViewProduct(item.id)}>
                  <img src={item.imageUrl} alt={item.name} loading="lazy" />
                  <div>
                    <strong>{item.name}</strong>
                    <small>{formatCurrency(item.price)}</small>
                  </div>
                </button>
              ))}
            </div>
            <Link to="/checkout">Review cart and payment</Link>
          </article>

          <article className="info-panel" id="orders">
            <span className="section-kicker">Recent orders</span>
            <h3>{orders.items.length} orders tracked</h3>
            <div className="order-mini-list">
              {orders.items.slice(0, 3).map((order) => (
                <div key={order.id} className="order-mini-card">
                  <div>
                    <strong>{order.orderNumber}</strong>
                    <small>{formatDate(order.orderDate)}</small>
                  </div>
                  <div>
                    <span>{order.trackingStage}</span>
                    <small>{formatCurrency(order.total)}</small>
                  </div>
                  <Link to={`/tracking/${order.id}`}>Track order</Link>
                </div>
              ))}
            </div>
            <Link to="/tracking">Open full tracking view</Link>
          </article>

          <article className="info-panel" id="admin">
            <span className="section-kicker">Admin dashboard</span>
            <h3>Operational snapshot</h3>
            <div className="stats-stack">
              <div>
                <small>Revenue</small>
                <strong>{formatCurrency(admin.grossRevenue)}</strong>
              </div>
              <div>
                <small>Orders</small>
                <strong>{admin.orderCount}</strong>
              </div>
              <div>
                <small>Average order</small>
                <strong>{formatCurrency(admin.averageOrderValue)}</strong>
              </div>
            </div>
          </article>
        </aside>
      </div>
    </section>
  </main>
  )
}

export default StorefrontPage

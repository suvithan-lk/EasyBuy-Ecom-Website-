import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'

const CheckoutPage = ({
  session,
  heroMedia,
  home,
  cart,
  couponCode,
  setCouponCode,
  couponPreview,
  applyingCoupon,
  checkoutPreview,
  checkoutForm,
  setCheckoutForm,
  deliveryLocation,
  setDeliveryLocation,
  districts,
  placingOrder,
  onCartQuantity,
  onApplyCoupon,
  onPlaceOrder,
  formatCurrency,
}) => {
  const navigate = useNavigate()
  const [paymentFields, setPaymentFields] = useState({
    cardNumber: '4111 1111 1111 1111',
    expiry: '12/28',
    cvv: '123',
    holder: session?.name ?? checkoutForm.name,
  })

  const submitCoupon = async (event) => {
    event.preventDefault()
    await onApplyCoupon()
  }

  const submitOrder = async (event) => {
    event.preventDefault()
    const result = await onPlaceOrder()
    navigate(`/tracking/${result.order.id}`)
  }

  return (
    <main className="checkout-page-shell">
      <section className="checkout-banner">
        <img src={heroMedia.deals} alt="Checkout page hero" />
        <div>
          <span className="section-kicker">Secure payment</span>
          <h1>Dedicated checkout and payment page</h1>
          <p>
            This replaces the old inline demo panel with a full address, payment, and order review
            flow.
          </p>
        </div>
      </section>

      <div className="checkout-layout">
        <form className="checkout-form-shell" onSubmit={submitOrder}>
          <section className="checkout-panel-card">
            <div className="panel-head">
              <div>
                <span className="section-kicker">Delivery address</span>
                <h2>Shipping details</h2>
              </div>
              {!session ? <Link to="/login">Login for faster checkout</Link> : null}
            </div>

            <div className="form-grid-two">
              <label>
                <span>Full name</span>
                <input
                  type="text"
                  value={checkoutForm.name}
                  onChange={(event) =>
                    setCheckoutForm((current) => ({ ...current, name: event.target.value }))
                  }
                />
              </label>
              <label>
                <span>Email</span>
                <input
                  type="email"
                  value={checkoutForm.email}
                  onChange={(event) =>
                    setCheckoutForm((current) => ({ ...current, email: event.target.value }))
                  }
                />
              </label>
              <label>
                <span>District</span>
                <select value={deliveryLocation} onChange={(event) => setDeliveryLocation(event.target.value)}>
                  {districts.map((district) => (
                    <option key={district} value={district}>
                      {district}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                <span>Postal code</span>
                <input
                  type="text"
                  value={checkoutForm.postalCode}
                  onChange={(event) =>
                    setCheckoutForm((current) => ({ ...current, postalCode: event.target.value }))
                  }
                />
              </label>
            </div>

            <label>
              <span>Street address</span>
              <input
                type="text"
                value={checkoutForm.addressLine}
                onChange={(event) =>
                  setCheckoutForm((current) => ({ ...current, addressLine: event.target.value }))
                }
              />
            </label>

            <label>
              <span>Delivery note</span>
              <textarea
                rows="3"
                value={checkoutForm.notes}
                onChange={(event) =>
                  setCheckoutForm((current) => ({ ...current, notes: event.target.value }))
                }
                placeholder="Leave at security desk or call before delivery"
              />
            </label>
          </section>

          <section className="checkout-panel-card">
            <div className="panel-head">
              <div>
                <span className="section-kicker">Payment method</span>
                <h2>How you want to pay</h2>
              </div>
            </div>

            <div className="payment-method-grid">
              {['Card', 'Cash on delivery', 'Bank transfer'].map((method) => (
                <button
                  key={method}
                  type="button"
                  className={`payment-option ${checkoutForm.paymentMethod === method ? 'active' : ''}`}
                  onClick={() =>
                    setCheckoutForm((current) => ({
                      ...current,
                      paymentMethod: method,
                    }))
                  }
                >
                  <strong>{method}</strong>
                  <small>
                    {method === 'Card'
                      ? 'Visa, Mastercard, Amex'
                      : method === 'Cash on delivery'
                        ? 'Pay at your doorstep'
                        : 'Manual confirmation after transfer'}
                  </small>
                </button>
              ))}
            </div>

            {checkoutForm.paymentMethod === 'Card' ? (
              <div className="form-grid-two payment-fields">
                <label>
                  <span>Cardholder name</span>
                  <input
                    type="text"
                    value={paymentFields.holder}
                    onChange={(event) =>
                      setPaymentFields((current) => ({ ...current, holder: event.target.value }))
                    }
                  />
                </label>
                <label>
                  <span>Card number</span>
                  <input
                    type="text"
                    value={paymentFields.cardNumber}
                    onChange={(event) =>
                      setPaymentFields((current) => ({
                        ...current,
                        cardNumber: event.target.value,
                      }))
                    }
                  />
                </label>
                <label>
                  <span>Expiry</span>
                  <input
                    type="text"
                    value={paymentFields.expiry}
                    onChange={(event) =>
                      setPaymentFields((current) => ({ ...current, expiry: event.target.value }))
                    }
                  />
                </label>
                <label>
                  <span>CVV</span>
                  <input
                    type="password"
                    value={paymentFields.cvv}
                    onChange={(event) =>
                      setPaymentFields((current) => ({ ...current, cvv: event.target.value }))
                    }
                  />
                </label>
              </div>
            ) : null}
          </section>

          <section className="checkout-panel-card">
            <div className="panel-head">
              <div>
                <span className="section-kicker">Payment action</span>
                <h2>Complete order</h2>
              </div>
            </div>
            <button type="submit" className="primary-action wide-action" disabled={placingOrder}>
              {placingOrder ? 'Processing payment...' : 'Place Order and Pay'}
            </button>
          </section>
        </form>

        <aside className="checkout-summary-shell">
          <article className="checkout-summary-card">
            <span className="section-kicker">Basket review</span>
            <h2>{cart.itemCount} items</h2>

            <div className="checkout-line-list">
              {cart.items.map((item) => (
                <div key={item.productId} className="checkout-line-item">
                  <img src={item.imageUrl} alt={item.name} loading="lazy" />
                  <div>
                    <strong>{item.name}</strong>
                    <small>{formatCurrency(item.unitPrice)}</small>
                    <div className="qty-actions">
                      <button type="button" onClick={() => onCartQuantity(item.productId, item.quantity - 1)}>
                        -
                      </button>
                      <span>{item.quantity}</span>
                      <button type="button" onClick={() => onCartQuantity(item.productId, item.quantity + 1)}>
                        +
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            <form className="coupon-inline-form" onSubmit={submitCoupon}>
              <input
                type="text"
                value={couponCode}
                onChange={(event) => setCouponCode(event.target.value.toUpperCase())}
                placeholder="Coupon code"
              />
              <button type="submit" disabled={applyingCoupon}>
                {applyingCoupon ? 'Checking...' : 'Apply'}
              </button>
            </form>

            {couponPreview ? (
              <p className="coupon-feedback">
                {couponPreview.code} active. You save {formatCurrency(couponPreview.discount)}.
              </p>
            ) : null}

            <div className="summary-table">
              <div>
                <span>Subtotal</span>
                <strong>{formatCurrency(checkoutPreview.subtotal)}</strong>
              </div>
              <div>
                <span>Discount</span>
                <strong>-{formatCurrency(checkoutPreview.discount)}</strong>
              </div>
              <div>
                <span>Shipping</span>
                <strong>{formatCurrency(checkoutPreview.shipping)}</strong>
              </div>
              <div className="summary-grand-total">
                <span>Total</span>
                <strong>{formatCurrency(checkoutPreview.total)}</strong>
              </div>
            </div>

            <div className="checkout-note">
              <strong>Free shipping threshold</strong>
              <small>Orders over {formatCurrency(home?.freeShippingThreshold ?? 0)} ship free.</small>
            </div>
          </article>
        </aside>
      </div>
    </main>
  )
}

export default CheckoutPage

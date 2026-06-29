import { ref } from 'vue';
import { API_BASE_URL, LEMON_SQUEEZY_CHECKOUT_URL } from '../config';
import { sendMockWebhook } from '../services/apiService';
import { MOCK_PAYMENT_QUANTITY, MOCK_PAYMENT_TOTAL } from '../constants';

export function usePayment() {
  const loading = ref(false);
  const error = ref('');

  const handleBuyCredits = async (userUid: string, refreshProfileCallback: () => Promise<void>) => {
    if (!userUid) {
      error.value = 'Please sign in first before purchasing credits.';
      return;
    }

    if (API_BASE_URL.includes('localhost') || API_BASE_URL.includes('127.0.0.1')) {
      loading.value = true;
      error.value = '';
      try {
        const payload = {
          meta: {
            event_name: 'order_created',
            custom_data: {
              user_id: userUid
            }
          },
          data: {
            id: 'mock-order-' + Date.now(),
            attributes: {
              first_order_item: {
                quantity: MOCK_PAYMENT_QUANTITY
              },
              total: MOCK_PAYMENT_TOTAL,
              currency: 'USD'
            }
          }
        };

        await sendMockWebhook(payload);
        await refreshProfileCallback();
      } catch (err) {
        const e = err as Error;
        error.value = 'Dev payment simulation failed: ' + e.message;
      } finally {
        loading.value = false;
      }
      return;
    }

    const checkoutUrl = `${LEMON_SQUEEZY_CHECKOUT_URL}?checkout[custom][user_id]=${userUid}&embed=1`;

    const lsWindow = window as any;
    if (lsWindow.LemonSqueezy) {
      lsWindow.LemonSqueezy.Url.Open(checkoutUrl);
    } else {
      window.open(checkoutUrl, '_blank');
    }
  };

  return {
    loading,
    error,
    handleBuyCredits
  };
}

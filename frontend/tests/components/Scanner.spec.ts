import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import Scanner from '../../src/components/Scanner.vue';
import { STORAGE_KEYS } from '../../src/config';

describe('Scanner.vue', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
    sessionStorage.clear();
  });

  it('renders initial textarea empty and placeholder text', () => {
    const wrapper = mount(Scanner);
    const textarea = wrapper.find('textarea');
    expect(textarea.element.value).toBe('');
  });

  it('handles successful scan and redirects', async () => {
    const mockResult = { riskLevel: 'GREEN', summary: 'Safe job', reasoning: 'All ok', factors: [] };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(mockResult)
    });
    vi.stubGlobal('fetch', fetchMock);
    
    // Mock window.location
    const locationMock = { href: '' };
    vi.stubGlobal('location', locationMock);

    const wrapper = mount(Scanner);
    const textarea = wrapper.find('textarea');
    await textarea.setValue('Fake job details');

    const form = wrapper.find('form');
    await form.trigger('submit');

    expect(fetchMock).toHaveBeenCalled();
    expect(sessionStorage.getItem(STORAGE_KEYS.SCAN_RESULT)).toContain('GREEN');
    expect(sessionStorage.getItem(STORAGE_KEYS.SCANNED_JOB_TEXT)).toBe('Fake job details');
  });

  it('handles failed scan and displays error message', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: false,
      text: () => Promise.resolve('API Limit Exceeded')
    });
    vi.stubGlobal('fetch', fetchMock);

    const wrapper = mount(Scanner);
    const textarea = wrapper.find('textarea');
    await textarea.setValue('Fake job details');

    const form = wrapper.find('form');
    await form.trigger('submit');

    await wrapper.vm.$nextTick();
    await new Promise((resolve) => setTimeout(resolve, 50));
    await wrapper.vm.$nextTick();

    expect(wrapper.text()).toContain('API Limit Exceeded');
  });

  it('handles dragover and dragleave events on dropzone', async () => {
    const wrapper = mount(Scanner);
    const dropzone = wrapper.find('div.border-dashed');
    expect(dropzone.exists()).toBe(true);

    await dropzone.trigger('dragover');
    expect(wrapper.vm.dragOver).toBe(true);

    await dropzone.trigger('dragleave');
    expect(wrapper.vm.dragOver).toBe(false);
  });

  it('handles file drop on dropzone and scans file', async () => {
    const mockResult = { riskLevel: 'YELLOW', summary: 'Suspicious job', reasoning: 'High salary', factors: [] };
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(mockResult)
    });
    vi.stubGlobal('fetch', fetchMock);
    
    const locationMock = { href: '' };
    vi.stubGlobal('location', locationMock);

    const wrapper = mount(Scanner);
    
    // Create a mock file
    const file = new File(['fake content'], 'test-job.pdf', { type: 'application/pdf' });
    
    // Mock the drop event data
    const dropEvent = {
      dataTransfer: {
        files: [file]
      }
    };
    
    const dropzone = wrapper.find('div.border-dashed');
    await dropzone.trigger('drop', dropEvent);
    
    expect(wrapper.vm.selectedFile).toBe(file);

    const form = wrapper.find('form');
    await form.trigger('submit');

    expect(fetchMock).toHaveBeenCalled();
    expect(sessionStorage.getItem(STORAGE_KEYS.SCAN_RESULT)).toContain('YELLOW');
  });

  it('triggers file input click on dropzone click', async () => {
    const wrapper = mount(Scanner);
    const clickMock = vi.fn();
    
    wrapper.vm.fileInput = { click: clickMock } as any;

    const dropzone = wrapper.find('div.border-dashed');
    await dropzone.trigger('click');
    expect(clickMock).toHaveBeenCalled();
  });

  it('handles file input change event', async () => {
    const wrapper = mount(Scanner);
    const file = new File(['fake content'], 'test-job.pdf', { type: 'application/pdf' });
    
    const fileInput = wrapper.find('input[type="file"]');
    
    Object.defineProperty(fileInput.element, 'files', {
      value: [file],
      writable: true
    });
    
    await fileInput.trigger('change');
    expect(wrapper.vm.selectedFile).toBe(file);
    expect(wrapper.vm.jobText).toBe('');
  });
});
